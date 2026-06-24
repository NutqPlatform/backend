using System;
using System.Collections.Generic;
using System.Linq;
using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Core.Models;

namespace Nutq.Core.Services
{
    /// <summary>
    /// Pure, stateless computation engine for therapy-plan-level analytics.
    /// Expanded with Clinical Intelligence & Decision Support logic.
    /// </summary>
    public static class PlanAnalyticsEngine
    {
        // ─── Top-level orchestrator ──────────────────────────────────────────────

        public static TherapyPlanAnalytics Compute(
            Nutq.Core.Entities.TherapyPlan plan,
            IReadOnlyList<TrainingSession> sessions,
            IReadOnlyList<SpeechAttempt> attempts,
            IReadOnlyList<SessionClinicalReport> reports,
            PlanProgressComparison progressComparison,
            PlanAnalyticsOptions options,
            IClinicalInsightGenerator insightGenerator,
            IReadOnlyList<PlanSessionEntry> sessionTimeline)
        {
            var words = BuildWordPerformances(attempts);
            var categories = BuildCategoryPerformances(words);
            var summary = BuildSummary(sessions, attempts, words, options);
            var strengths = BuildStrengths(words, categories, options);
            var weaknesses = BuildWeaknesses(words, categories, attempts, options);
            var recurringDifficulties = BuildRecurringDifficulties(attempts);
            var suggestedNextContent = BuildSuggestedNextContent(summary, words, categories, plan.PlanExercises?.Count ?? 4);

            // Delegate clinical insights generation to the abstraction layer
            var insights = insightGenerator.GenerateInsights(words, categories, reports, strengths, weaknesses);

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
                insights,
                recurringDifficulties,
                suggestedNextContent,
                sessionTimeline);
        }

        // ─── Summary ─────────────────────────────────────────────────────────────

        public static TherapyPlanSummary BuildSummary(
            IReadOnlyList<TrainingSession> sessions,
            IReadOnlyList<SpeechAttempt> attempts,
            IReadOnlyList<PlanWordPerformance> words,
            PlanAnalyticsOptions options)
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

            // 1. Mastered Similarity (Average of best similarity score per word)
            var masteredSimilarity = words.Any()
                ? Round(words.Average(w => w.BestSimilarityScore)) : 0;

            // 2. Plan Outcome Score (Configurable Weighted Score)
            var rawScore = (options.WsrWeight * wordSuccessRate)
                + (options.AvgSimilarityWeight * avgSimilarity)
                + (options.FasrWeight * firstAttemptRate)
                - (options.FailedWordPenalty * failedWords);

            var outcomeScore = Math.Clamp(Round(rawScore), 0, 100);

            // 3. Plan Outcome Rating
            string outcomeRating;
            if (outcomeScore >= options.ExcellentThreshold) outcomeRating = "Excellent";
            else if (outcomeScore >= options.GoodThreshold) outcomeRating = "Good";
            else if (outcomeScore >= options.ModerateThreshold) outcomeRating = "Moderate";
            else outcomeRating = "Needs Attention";

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
                sessions.Count,
                masteredSimilarity,
                outcomeScore,
                outcomeRating);
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
                    first.ExpectedWord,
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
            IReadOnlyList<PlanCategoryPerformance> categories,
            PlanAnalyticsOptions options)
        {
            var bestWords = words
                .Where(w => w.FinalSuccess)
                .OrderByDescending(w => w.BestSimilarityScore)
                .ThenBy(w => w.TotalAttempts)
                .Take(10)
                .ToList();

            var bestCategories = categories
                .Where(c => c.AccuracyPercent >= options.GoodThreshold)
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
            IReadOnlyList<PlanCategoryPerformance> categories,
            IReadOnlyList<SpeechAttempt> attempts,
            PlanAnalyticsOptions options)
        {
            var failedWords = words
                .Where(w => !w.FinalSuccess)
                .OrderBy(w => w.BestSimilarityScore)
                .ToList();

            var highRetryWords = words
                .Where(w => w.TotalAttempts > options.HighRetryWordThreshold)
                .OrderByDescending(w => w.TotalAttempts)
                .Take(10)
                .ToList();

            var weakCategories = categories
                .Where(c => c.AccuracyPercent < options.GoodThreshold)
                .OrderBy(c => c.AccuracyPercent)
                .ToList();

            var lowSimilarityWords = words
                .Where(w => w.BestSimilarityScore < options.LowSimilarityWordThreshold)
                .OrderBy(w => w.BestSimilarityScore)
                .Take(10)
                .ToList();

            var recurringDifficulties = weakCategories
                .Where(c => c.AverageAttemptsPerWord > options.HighRetryWordThreshold)
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
            double? refFirstAttempt,
            PlanAnalyticsOptions options)
        {
            if (refAccuracy == null)
                return new PlanPeriodComparison(period, null, null, null, false, "Stable");

            var accuracyDelta = Round(currentAccuracy - refAccuracy.Value);
            var similarityDelta = refSimilarity.HasValue ? (double?)Round(currentSimilarity - refSimilarity.Value) : null;
            var firstAttemptDelta = refFirstAttempt.HasValue ? (double?)Round(currentFirstAttempt - refFirstAttempt.Value) : null;

            // Trend Rating calculation
            string trendRating = "Stable";
            if (accuracyDelta >= options.StrongImprovementThreshold)
                trendRating = "Strong Improvement";
            else if (accuracyDelta >= options.ImprovingThreshold)
                trendRating = "Improving";
            else if (accuracyDelta <= options.CriticalDeclineThreshold)
                trendRating = "Critical Decline";
            else if (accuracyDelta <= options.DecliningThreshold)
                trendRating = "Declining";

            return new PlanPeriodComparison(
                period,
                accuracyDelta,
                similarityDelta,
                firstAttemptDelta,
                true,
                trendRating);
        }

        // ─── Recurring Difficulty Detection ──────────────────────────────────────

        public static IReadOnlyList<RecurringDifficultyItem> BuildRecurringDifficulties(
            IReadOnlyList<SpeechAttempt> attempts)
        {
            var wordGroups = attempts.GroupBy(a => a.ExpectedWord).ToList();
            var result = new List<RecurringDifficultyItem>();

            foreach (var g in wordGroups)
            {
                var word = g.Key;
                var category = g.First().Category;

                var sessionGroups = g.GroupBy(a => a.TrainingSessionId).ToList();
                int failedSessionsCount = 0;
                double totalAttemptsAcrossSessions = 0;

                foreach (var sg in sessionGroups)
                {
                    bool succeededInSession = sg.Any(a => a.IsCorrect);
                    if (!succeededInSession)
                    {
                        failedSessionsCount++;
                    }
                    totalAttemptsAcrossSessions += sg.Count();
                }

                double avgAttemptsPerSession = sessionGroups.Any()
                    ? totalAttemptsAcrossSessions / sessionGroups.Count
                    : 0;

                double bestSimilarity = g.Any() ? g.Max(a => a.SimilarityScore) : 0;

                // Severity Score Calculation
                double severityScore = Math.Max(0, avgAttemptsPerSession - 1.0) * 20.0
                    + (100.0 - bestSimilarity)
                    + (failedSessionsCount * 15.0);

                severityScore = Math.Clamp(Math.Round(severityScore, 2), 0, 100);

                // Attention Level Allocation
                string attentionLevel = "Low";
                if (severityScore >= 75 || failedSessionsCount >= 3)
                {
                    attentionLevel = "High";
                }
                else if (severityScore >= 40 || failedSessionsCount >= 2)
                {
                    attentionLevel = "Medium";
                }

                // Flag if word has triggered failure, high severity or average attempts > 2
                if (severityScore >= 30 || failedSessionsCount > 0 || avgAttemptsPerSession > 2.0)
                {
                    result.Add(new RecurringDifficultyItem(
                        word,
                        category,
                        failedSessionsCount,
                        severityScore,
                        attentionLevel));
                }
            }

            return result.OrderByDescending(r => r.SeverityScore).ToList();
        }

        // ─── Suggested Next Content ──────────────────────────────────────────────

        public static SuggestedNextTherapyContent BuildSuggestedNextContent(
            TherapyPlanSummary summary,
            IReadOnlyList<PlanWordPerformance> words,
            IReadOnlyList<PlanCategoryPerformance> categories,
            int currentExerciseCount)
        {
            var categoriesNeedingReinforcement = categories
                .Where(c => c.AccuracyPercent < 70)
                .Select(c => c.Category)
                .ToList();

            var vocabularyNeedingRepetition = words
                .Where(w => !w.FinalSuccess || w.TotalAttempts > 2)
                .Select(w => w.Word)
                .ToList();

            // Difficulty adjustment
            string difficultyAdjustment = "Maintain level (continue current complexity)";
            if (summary.PlanOutcomeScore >= 85)
            {
                difficultyAdjustment = "Increase difficulty (introduce longer words / advanced syntax)";
            }
            else if (summary.PlanOutcomeScore < 50)
            {
                difficultyAdjustment = "Reduce difficulty (focus on single syllable words / shorter lists)";
            }

            // Exercise count advice
            int recommendedExerciseCount = currentExerciseCount;
            if (summary.PlanOutcomeScore >= 70)
            {
                recommendedExerciseCount = Math.Min(6, currentExerciseCount + 1);
            }
            else if (summary.PlanOutcomeScore < 50)
            {
                recommendedExerciseCount = Math.Max(2, currentExerciseCount - 1);
            }

            // Reasoning phrasing
            string reasoning;
            if (summary.PlanOutcomeScore >= 85)
            {
                reasoning = $"Excellent overall rating ({summary.PlanOutcomeScore}% score). Patient showed rapid phonetic mastery, indicating readiness for a higher difficulty tier and additional exercises.";
            }
            else if (summary.PlanOutcomeScore >= 70)
            {
                reasoning = $"Good progress ({summary.PlanOutcomeScore}% score). General articulation is stable; continue current difficulty but add an extra reinforcement exercise.";
            }
            else if (summary.PlanOutcomeScore >= 50)
            {
                reasoning = $"Moderate capability ({summary.PlanOutcomeScore}% score). Mild phonetic struggles in {categoriesNeedingReinforcement.Count} categories. Suggest keeping list sizes stable and repeating failed words.";
            }
            else
            {
                reasoning = $"Needs attention ({summary.PlanOutcomeScore}% score). Word success rate was at {summary.WordSuccessRate}% with {summary.TotalFailedWords} failed words. Recommended to simplify target vocabulary and shorten lists.";
            }

            return new SuggestedNextTherapyContent(
                categoriesNeedingReinforcement,
                vocabularyNeedingRepetition,
                difficultyAdjustment,
                recommendedExerciseCount,
                reasoning);
        }

        // ─── Helpers ─────────────────────────────────────────────────────────────

        private static double Round(double value) => Math.Round(value, 2);
    }
}
