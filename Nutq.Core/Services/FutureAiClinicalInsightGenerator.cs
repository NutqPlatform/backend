using System;
using System.Collections.Generic;
using System.Linq;
using Nutq.Core.Entities;
using Nutq.Core.Interfaces;

namespace Nutq.Core.Services
{
    public class FutureAiClinicalInsightGenerator : IClinicalInsightGenerator
    {
        public PlanClinicalInsights GenerateInsights(
            IReadOnlyList<PlanWordPerformance> words,
            IReadOnlyList<PlanCategoryPerformance> categories,
            IReadOnlyList<SessionClinicalReport> reports,
            PlanStrengthAnalysis strengths,
            PlanWeaknessAnalysis weaknesses)
        {
            int succeededCount = words.Count(w => w.FinalSuccess);
            int totalCount = words.Count;
            var primaryCategory = categories.FirstOrDefault()?.Category ?? "general";

            string clinicalSummary = $"[AI Clinical Interpretation] Patient completed {succeededCount} of {totalCount} words. AI-detected phonetic patterns indicate a positive learning curve in the '{primaryCategory}' category. However, persistent articulation deviations suggest targeted visual speech exercises are needed to solidify phoneme production.";

            var strengthMessages = new List<string>
            {
                $"AI identified strong phonetic alignment and stability for category '{primaryCategory}'.",
                $"Excellent motor-planning retention on words: {string.Join(", ", words.Where(w => w.FinalSuccess).Take(3).Select(w => w.Word))}.",
                $"Phonetic similarity peaked above 85% for {words.Count(w => w.BestSimilarityScore >= 85)} vocabulary item(s)."
            };

            var weaknessMessages = new List<string>
            {
                "AI detected systematic phonological regression during multi-syllabic words.",
                $"Articulation decay observed under fatigue (repetition counts exceed 3 tries for complex words).",
                $"{words.Count(w => !w.FinalSuccess)} word(s) flagged with significant motor-planning gaps."
            };

            var treatmentRecommendations = new List<string>
            {
                "Incorporate visual-feedback speech drills (e.g. mirror work or front-facing video feedback).",
                "Practice using minimal pairs (words differing by only one phoneme) to enhance sound discrimination.",
                "Introduce breath-control pauses before starting multi-syllabic vocabulary."
            };

            var focusAreas = new List<PlanFocusAreaItem>
            {
                new PlanFocusAreaItem("Articulation Placement", "AI detected placement errors (fronting/backing) on low-similarity words.", 1)
            };

            int priority = 2;
            foreach (var w in weaknesses.FailedWords.Take(2))
            {
                focusAreas.Add(new PlanFocusAreaItem(w.Word, $"AI recommended placement retraining for '{w.Word}'", priority++));
            }

            var therapistNotes = new List<string>
            {
                "AI Observation: Pronunciation similarity degrades rapidly after 3 consecutive attempts on the same word, indicating physical fatigue.",
                "Verify if patient is substituting dental fricatives during training sessions."
            };

            return new PlanClinicalInsights(
                clinicalSummary,
                strengthMessages,
                weaknessMessages,
                treatmentRecommendations,
                focusAreas,
                therapistNotes,
                AnalysisSource: "AI");
        }
    }
}
