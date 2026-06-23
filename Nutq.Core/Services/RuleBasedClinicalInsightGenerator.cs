using System;
using System.Collections.Generic;
using System.Linq;
using Nutq.Core.Entities;
using Nutq.Core.Interfaces;

namespace Nutq.Core.Services
{
    public class RuleBasedClinicalInsightGenerator : IClinicalInsightGenerator
    {
        private const double StrengthThreshold = 70.0;
        private const double LowSimilarityThreshold = 60.0;
        private const double HighRetryThreshold = 2.0;

        public PlanClinicalInsights GenerateInsights(
            IReadOnlyList<PlanWordPerformance> words,
            IReadOnlyList<PlanCategoryPerformance> categories,
            IReadOnlyList<SessionClinicalReport> reports,
            PlanStrengthAnalysis strengths,
            PlanWeaknessAnalysis weaknesses)
        {
            var strengthMessages = new List<string>();
            var weaknessMessages = new List<string>();
            var focusAreas = new List<PlanFocusAreaItem>();
            var treatmentRecommendations = new List<string>();
            var therapistNotes = new List<string>();

            int succeededCount = words.Count(w => w.FinalSuccess);
            int totalCount = words.Count;
            double successRate = totalCount > 0 ? (double)succeededCount / totalCount * 100 : 0;

            // 1. Clinical Summary
            string clinicalSummary;
            if (successRate >= 85)
            {
                clinicalSummary = $"The patient demonstrated excellent speech patterns and high phonetic accuracy throughout this plan, mastering {succeededCount} of {totalCount} vocabulary items. Articulation is highly consistent.";
            }
            else if (successRate >= 70)
            {
                clinicalSummary = $"The patient shows good progress with solid articulation on most target vocabulary ({succeededCount}/{totalCount} succeeded). Certain complex syllable transitions require continued drills.";
            }
            else if (successRate >= 50)
            {
                clinicalSummary = $"The patient exhibits moderate speech capability, completing {succeededCount} of {totalCount} words. Frequent repetition and articulation structure adjustments are recommended to improve phonetic retention.";
            }
            else
            {
                clinicalSummary = "Treatment needs immediate attention. High retry rates and poor phonetic similarity suggest significant articulation struggles with the current plan's vocabulary.";
            }

            // 2. Strengths Analysis
            foreach (var cat in strengths.BestPerformingCategories.Take(3))
            {
                strengthMessages.Add($"Strong performance in '{cat.Category}' ({cat.AccuracyPercent:0.#}% accuracy).");
            }

            if (strengths.MasteredOnFirstAttempt.Count > 0)
            {
                strengthMessages.Add($"{strengths.MasteredOnFirstAttempt.Count} word(s) mastered on the first attempt — excellent phonetic retention.");
            }

            if (strengthMessages.Count == 0 && succeededCount > 0)
            {
                strengthMessages.Add($"{succeededCount} word(s) successfully completed during this plan.");
            }

            // 3. Weakness Analysis
            foreach (var cat in weaknesses.LowPerformanceCategories.Take(3))
            {
                weaknessMessages.Add($"Category '{cat.Category}' requires attention ({cat.AccuracyPercent:0.#}% accuracy, avg {cat.AverageAttemptsPerWord:0.#} attempts/word).");
            }

            if (weaknesses.FailedWords.Count > 0)
            {
                weaknessMessages.Add($"{weaknesses.FailedWords.Count} word(s) could not be completed successfully.");
            }

            if (weaknessMessages.Count == 0)
            {
                weaknessMessages.Add("No significant weaknesses identified in this plan.");
            }

            // 4. Treatment Recommendations
            foreach (var cat in weaknesses.LowPerformanceCategories.Take(2))
            {
                treatmentRecommendations.Add($"Additional '{cat.Category}' vocabulary exercises with focused repetition.");
            }

            if (weaknesses.HighRetryWords.Count > 0)
            {
                treatmentRecommendations.Add("Slow-paced word drills targeting high-retry vocabulary.");
            }

            if (treatmentRecommendations.Count == 0)
            {
                treatmentRecommendations.Add("Continue with next-level vocabulary in current categories.");
            }

            // 5. Suggested Focus Areas
            var priority = 1;
            foreach (var cat in weaknesses.LowPerformanceCategories.Take(3))
            {
                var rationale = cat.AverageAttemptsPerWord > HighRetryThreshold
                    ? $"High retry rate ({cat.AverageAttemptsPerWord:0.#} attempts/word) in '{cat.Category}'"
                    : $"Low accuracy ({cat.AccuracyPercent:0.#}%) in '{cat.Category}'";
                focusAreas.Add(new PlanFocusAreaItem(cat.Category, rationale, priority++));
            }

            foreach (var w in weaknesses.FailedWords.Take(5))
            {
                focusAreas.Add(new PlanFocusAreaItem(
                    w.Word,
                    $"Word '{w.Word}' not yet mastered (best similarity {w.BestSimilarityScore:0.##}%)",
                    priority++));
            }

            // 6. Therapist Notes
            foreach (var w in weaknesses.LowSimilarityWords.Take(3))
            {
                therapistNotes.Add($"Word '{w.Word}' shows poor phonetic match (best similarity {w.BestSimilarityScore:0.##}%). Manual articulation therapy recommended.");
            }

            foreach (var d in weaknesses.RecurringDifficulties.Take(2))
            {
                therapistNotes.Add($"Recurring difficulty: {d}.");
            }

            // Merge report-based patterns if available
            foreach (var report in reports)
            {
                if (!string.IsNullOrWhiteSpace(report.WeaknessAreasJson))
                {
                    try
                    {
                        var areas = System.Text.Json.JsonSerializer.Deserialize<List<string>>(report.WeaknessAreasJson);
                        if (areas != null)
                        {
                            foreach (var area in areas.Where(a => !therapistNotes.Any(t => t.Contains(a))).Take(2))
                            {
                                therapistNotes.Add($"Recurring pronunciation difficulty in: {area}.");
                            }
                        }
                    }
                    catch { /* ignore malformed json */ }
                }
            }

            return new PlanClinicalInsights(
                clinicalSummary,
                strengthMessages,
                weaknessMessages,
                treatmentRecommendations,
                focusAreas,
                therapistNotes,
                AnalysisSource: "RuleBased");
        }
    }
}
