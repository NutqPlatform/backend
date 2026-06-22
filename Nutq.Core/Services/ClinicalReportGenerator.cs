using System.Text.Json;
using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Core.Models;

namespace Nutq.Core.Services
{
    public class ClinicalReportGenerator
    {
        public SessionClinicalReport Generate(
            int patientId,
            TrainingSession session,
            SessionMetrics metrics,
            IReadOnlyList<CategoryMetrics> categoryMetrics,
            IEnumerable<PronunciationPattern> activePatterns)
        {
            var ordered = categoryMetrics
                .Where(c => c.WordsAttempted > 0)
                .OrderByDescending(c => c.AccuracyPercent)
                .ToList();

            var strengths = ordered
                .Where(c => c.AccuracyPercent >= 70)
                .Take(3)
                .Select(c => c.Category)
                .ToList();

            if (strengths.Count == 0 && ordered.Count > 0)
                strengths = ordered.Take(1).Select(c => c.Category).ToList();

            var weaknesses = ordered
                .Where(c => c.AccuracyPercent < 70)
                .OrderBy(c => c.AccuracyPercent)
                .Take(3)
                .Select(c => c.Category)
                .ToList();

            if (weaknesses.Count == 0 && ordered.Count > 1)
                weaknesses = ordered.TakeLast(1).Select(c => c.Category).ToList();

            var recommendations = BuildRecommendations(ordered, activePatterns);

            return new SessionClinicalReport
            {
                PatientId = patientId,
                TrainingSessionId = session.Id,
                SessionDurationSeconds = session.TotalDurationSeconds,
                ExercisesCompleted = session.WordsCompleted,
                AccuracyRate = metrics.AccuracyPercent,
                FirstAttemptSuccessRate = metrics.FirstAttemptSuccessRate,
                AveragePronunciationSimilarity = metrics.AverageSimilarityScore,
                StrengthAreasJson = JsonSerializer.Serialize(strengths),
                WeaknessAreasJson = JsonSerializer.Serialize(weaknesses),
                RecommendedFocusJson = JsonSerializer.Serialize(recommendations),
                AnalysisSource = AnalyticsHelpers.AnalysisSourceDeterministic,
                GeneratedAt = DateTime.UtcNow
            };
        }

        private static List<object> BuildRecommendations(
            List<CategoryMetrics> ordered,
            IEnumerable<PronunciationPattern> activePatterns)
        {
            var items = new List<object>();
            var priority = 1;

            foreach (var weak in ordered.Where(c => c.AccuracyPercent < 70 || c.AverageAttemptsPerWord > 2)
                         .OrderBy(c => c.AccuracyPercent)
                         .Take(3))
            {
                items.Add(new
                {
                    category = weak.Category,
                    rationale = weak.AverageAttemptsPerWord > 2
                        ? $"High retry rate ({weak.AverageAttemptsPerWord:0.##} attempts/word) in {weak.Category}"
                        : $"Low accuracy ({weak.AccuracyPercent:0.##}%) in {weak.Category}",
                    priority = priority++
                });
            }

            foreach (var pattern in activePatterns.OrderByDescending(p => p.SeverityScore).Take(2))
            {
                items.Add(new
                {
                    category = pattern.Category ?? "pronunciation",
                    rationale = $"Recurring {pattern.PatternType.ToLower()} pattern: expected '{pattern.ExpectedPattern}', often heard as '{pattern.RecognizedPattern}' ({pattern.OccurrenceCount}x)",
                    priority = priority++
                });
            }

            return items;
        }
    }
}
