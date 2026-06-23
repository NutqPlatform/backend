using Nutq.Core.Entities;
using Nutq.Core.Interfaces;

namespace Nutq.Core.Services
{
    /// <summary>
    /// Pure, stateless computation engine for therapy-plan-level analytics.
    /// Contains no I/O — all inputs are pre-loaded lists passed in from the service layer.
    /// Designed for AI-readiness: the output shape of <see cref="PlanClinicalInsights"/> is
    /// identical whether produced by this deterministic engine or an AI provider.
    /// </summary>
    public static class PlanAnalyticsEngine
    {
        private const double StrengthThreshold = 70.0;
        private const double LowSimilarityThreshold = 0.6;
        private const double HighRetryThreshold = 2.0;

        // ─── Top-level orchestrator ──────────────────────────────────────────────

        public static TherapyPlanAnalytics Compute(
            Nutq.Core.Entities.TherapyPlan plan,
            IReadOnlyList<TrainingSession> sessions,
            IReadOnlyList<SpeechAttempt> attempts,
            IReadOnlyList<SessionClinicalReport> reports,
            PlanProgressComparison progressComparison)
        {
            var words = BuildWordPerformances(attempts);
            var categories = BuildCategoryPerformances(words);
            var summary = BuildSummary(sessions, attempts, words);
            var strengths = BuildStrengths(words, categories);
            var weaknesses = BuildWeaknesses(words, categories);
            var insights = BuildClinicalInsights(strengths, weaknesses, words, categories, reports);

            return new TherapyPlanAnalytics(
                plan.Id,
                plan.PatientId,
                plan.Description ?? "Untitled Plan",
                plan.Status ?? "Unknown",
                plan.StartDate,
                plan.EndDate,
                summary,
                words,
                categories,
                strengths,
                weaknesses,
                progressComparison,
                insights);
        }

        // ─── Summary ─────────────────────────────────────────────────────────────

        public static TherapyPlanSummary BuildSummary(
            IReadOnlyList<TrainingSession> sessions,
            IReadOnlyList<SpeechAttempt> attempts,
            IReadOnlyList<PlanWordPerformance> words)
        {
            var totalDuration = sessions.Sum(s => s.TotalDurationSeconds);
            var totalAttempts = attempts.Count;
            var totalWords = words.Count;
            var succeededWords = words.Count(w => w.FinalSuccess);
            var failedWords = totalWords - succeededWords;

            var wordSuccessRate = totalWords > 0
                ? Round((double)succeededWords / totalWords * 100) : 0;

            var correctAttempts = attempts.Count(a => a.IsCorrect);
            var attemptAccuracy = totalAttempts > 0
                ? Round((double)correctAttempts / totalAttempts * 100) : 0;

            var firstAttemptWords = words.Count(w => w.FirstAttemptSuccess);
            var firstAttemptRate = totalWords > 0
                ? Round((double)firstAttemptWords / totalWords * 100) : 0;

            var avgSimilarity = totalAttempts > 0
                ? Round(attempts.Average(a => a.SimilarityScore)) : 0;

            return new TherapyPlanSummary(
                totalDuration,
                totalWords,
                totalAttempts,
                wordSuccessRate,
                attemptAccuracy,
                firstAttemptRate,
                avgSimilarity,
                failedWords,
                succeededWords,
                sessions.Count);
        }

        // ─── Word-level breakdown ────────────────────────────────────────────────

        public static IReadOnlyList<PlanWordPerformance> BuildWordPerformances(
            IReadOnlyList<SpeechAttempt> attempts)
        {
            var groups = attempts
                .GroupBy(a => $"{a.VocabularyId}|{a.ExpectedWord}")
                .ToList();

            var result = new List<PlanWordPerformance>(groups.Count);

            foreach (var g in groups)
            {
                var ordered = g.OrderBy(a => a.AttemptNumber).ThenBy(a => a.AttemptedAt).ToList();
                var first = ordered[0];
                var bestSimilarity = Round(ordered.Max(a => a.SimilarityScore));
                var avgSimilarity = Round(ordered.Average(a => a.SimilarityScore));
                var timeSpent = ordered.Sum(a => a.AudioDurationSeconds);
                var finalSuccess = ordered.Any(a => a.IsCorrect);

                var history = ordered
                    .Select(a => new RecognizedWordEntry(
                        a.AttemptNumber,
                        a.RecognizedWord,
                        Round(a.SimilarityScore),
                        a.IsCorrect))
                    .ToList();

                result.Add(new PlanWordPerformance(
                    first.ExpectedWord,
                    first.ExpectedWord,   // enriched in service if vocab available
                    first.ExpectedWord,
                    first.Category,
                    ordered.Count,
                    bestSimilarity,
                    avgSimilarity,
                    first.IsCorrect,
                    finalSuccess,
                    Round(timeSpent),
                    history));
            }

            return result
                .OrderByDescending(w => w.TotalAttempts)
                .ThenBy(w => w.BestSimilarityScore)
                .ToList();
        }

        // ─── Category breakdown ──────────────────────────────────────────────────

        public static IReadOnlyList<PlanCategoryPerformance> BuildCategoryPerformances(
            IReadOnlyList<PlanWordPerformance> words)
        {
            return words
                .GroupBy(w => w.Category ?? "Uncategorized")
                .Select(g =>
                {
                    var list = g.ToList();
                    var succeeded = list.Count(w => w.FinalSuccess);
                    var accuracy = list.Count > 0 ? Round((double)succeeded / list.Count * 100) : 0;
                    var avgSim = list.Count > 0 ? Round(list.Average(w => w.AverageSimilarityScore)) : 0;
                    var avgAttempts = list.Count > 0 ? Round(list.Average(w => w.TotalAttempts)) : 0;
                    return new PlanCategoryPerformance(
                        g.Key,
                        list.Count,
                        succeeded,
                        accuracy,
                        avgSim,
                        avgAttempts);
                })
                .OrderByDescending(c => c.AccuracyPercent)
                .ToList();
        }

        // ─── Strengths ───────────────────────────────────────────────────────────

        public static PlanStrengthAnalysis BuildStrengths(
            IReadOnlyList<PlanWordPerformance> words,
            IReadOnlyList<PlanCategoryPerformance> categories)
        {
            var bestWords = words
                .Where(w => w.FinalSuccess)
                .OrderByDescending(w => w.BestSimilarityScore)
                .ThenBy(w => w.TotalAttempts)
                .Take(10)
                .ToList();

            var bestCategories = categories
                .Where(c => c.AccuracyPercent >= StrengthThreshold)
                .OrderByDescending(c => c.AccuracyPercent)
                .Take(5)
                .ToList();

            var masteredFirst = words
                .Where(w => w.FirstAttemptSuccess)
                .OrderByDescending(w => w.BestSimilarityScore)
                .Take(10)
                .ToList();

            var strongAreas = bestCategories
                .Select(c => c.Category)
                .ToList();

            return new PlanStrengthAnalysis(bestWords, bestCategories, masteredFirst, strongAreas);
        }

        // ─── Weaknesses ──────────────────────────────────────────────────────────

        public static PlanWeaknessAnalysis BuildWeaknesses(
            IReadOnlyList<PlanWordPerformance> words,
            IReadOnlyList<PlanCategoryPerformance> categories)
        {
            var failedWords = words
                .Where(w => !w.FinalSuccess)
                .OrderBy(w => w.BestSimilarityScore)
                .ToList();

            var highRetryWords = words
                .Where(w => w.TotalAttempts > HighRetryThreshold)
                .OrderByDescending(w => w.TotalAttempts)
                .Take(10)
                .ToList();

            var weakCategories = categories
                .Where(c => c.AccuracyPercent < StrengthThreshold)
                .OrderBy(c => c.AccuracyPercent)
                .ToList();

            var lowSimilarityWords = words
                .Where(w => w.BestSimilarityScore < LowSimilarityThreshold)
                .OrderBy(w => w.BestSimilarityScore)
                .Take(10)
                .ToList();

            var recurringDifficulties = weakCategories
                .Where(c => c.AverageAttemptsPerWord > HighRetryThreshold)
                .Select(c => $"{c.Category} (avg {c.AverageAttemptsPerWord:0.#} attempts/word)")
                .ToList();

            return new PlanWeaknessAnalysis(failedWords, highRetryWords, weakCategories, lowSimilarityWords, recurringDifficulties);
        }

        // ─── Progress comparison ─────────────────────────────────────────────────

        public static PlanPeriodComparison BuildPeriodComparison(
            string period,
            double currentAccuracy,
            double currentSimilarity,
            double currentFirstAttempt,
            double? refAccuracy,
            double? refSimilarity,
            double? refFirstAttempt)
        {
            if (refAccuracy == null)
                return new PlanPeriodComparison(period, null, null, null, false);

            return new PlanPeriodComparison(
                period,
                Round(currentAccuracy - refAccuracy.Value),
                refSimilarity.HasValue ? Round(currentSimilarity - refSimilarity.Value) : null,
                refFirstAttempt.HasValue ? Round(currentFirstAttempt - refFirstAttempt.Value) : null,
                true);
        }

        // ─── Clinical Insights (Deterministic V1) ────────────────────────────────

        public static PlanClinicalInsights BuildClinicalInsights(
            PlanStrengthAnalysis strengths,
            PlanWeaknessAnalysis weaknesses,
            IReadOnlyList<PlanWordPerformance> words,
            IReadOnlyList<PlanCategoryPerformance> categories,
            IReadOnlyList<SessionClinicalReport> reports)
        {
            var strengthMessages = new List<string>();
            var weaknessMessages = new List<string>();
            var focusAreas = new List<PlanFocusAreaItem>();
            var suggestedExercises = new List<string>();
            var therapyAttention = new List<string>();

            // Strengths
            foreach (var cat in strengths.BestPerformingCategories.Take(3))
                strengthMessages.Add($"Strong performance in '{cat.Category}' ({cat.AccuracyPercent:0.#}% accuracy).");

            if (strengths.MasteredOnFirstAttempt.Count > 0)
                strengthMessages.Add($"{strengths.MasteredOnFirstAttempt.Count} word(s) mastered on the first attempt — excellent phonetic retention.");

            if (strengthMessages.Count == 0 && words.Any(w => w.FinalSuccess))
                strengthMessages.Add($"{words.Count(w => w.FinalSuccess)} word(s) successfully completed during this plan.");

            // Weaknesses
            foreach (var cat in weaknesses.LowPerformanceCategories.Take(3))
                weaknessMessages.Add($"Category '{cat.Category}' requires attention ({cat.AccuracyPercent:0.#}% accuracy, avg {cat.AverageAttemptsPerWord:0.#} attempts/word).");

            if (weaknesses.FailedWords.Count > 0)
                weaknessMessages.Add($"{weaknesses.FailedWords.Count} word(s) could not be completed successfully.");

            if (weaknessMessages.Count == 0)
                weaknessMessages.Add("No significant weaknesses identified in this plan.");

            // Focus areas
            var priority = 1;
            foreach (var cat in weaknesses.LowPerformanceCategories.Take(3))
            {
                var rationale = cat.AverageAttemptsPerWord > HighRetryThreshold
                    ? $"High retry rate ({cat.AverageAttemptsPerWord:0.#} attempts/word) in '{cat.Category}'"
                    : $"Low accuracy ({cat.AccuracyPercent:0.#}%) in '{cat.Category}'";
                focusAreas.Add(new PlanFocusAreaItem(cat.Category, rationale, priority++));
            }

            foreach (var w in weaknesses.FailedWords.Take(5))
                focusAreas.Add(new PlanFocusAreaItem(
                    w.Word,
                    $"Word '{w.Word}' not yet mastered (best similarity {w.BestSimilarityScore:0.##})",
                    priority++));

            // Suggested exercises
            foreach (var cat in weaknesses.LowPerformanceCategories.Take(2))
                suggestedExercises.Add($"Additional '{cat.Category}' vocabulary exercises with focused repetition.");

            if (weaknesses.HighRetryWords.Count > 0)
                suggestedExercises.Add("Slow-paced word drills targeting high-retry vocabulary.");

            if (suggestedExercises.Count == 0)
                suggestedExercises.Add("Continue with next-level vocabulary in current categories.");

            // Therapy attention
            foreach (var w in weaknesses.LowSimilarityWords.Take(3))
                therapyAttention.Add($"Word '{w.Word}' shows poor phonetic match (best similarity {w.BestSimilarityScore:0.##}). Manual articulation therapy recommended.");

            foreach (var d in weaknesses.RecurringDifficulties.Take(2))
                therapyAttention.Add($"Recurring difficulty: {d}.");

            // Merge report-based patterns if available
            foreach (var report in reports)
            {
                if (!string.IsNullOrWhiteSpace(report.WeaknessAreasJson))
                {
                    try
                    {
                        var areas = System.Text.Json.JsonSerializer.Deserialize<List<string>>(report.WeaknessAreasJson);
                        if (areas != null)
                            foreach (var area in areas.Where(a => !therapyAttention.Any(t => t.Contains(a))).Take(2))
                                therapyAttention.Add($"Recurring pronunciation difficulty in: {area}.");
                    }
                    catch { /* ignore malformed json */ }
                }
            }

            return new PlanClinicalInsights(
                strengthMessages,
                weaknessMessages,
                focusAreas,
                suggestedExercises,
                therapyAttention,
                AnalysisSource: "Deterministic");
        }

        // ─── Helpers ─────────────────────────────────────────────────────────────

        private static double Round(double value) => Math.Round(value, 2);
    }
}
