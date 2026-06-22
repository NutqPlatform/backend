using Nutq.Core.Models;

namespace Nutq.Core.Services
{
    public record SessionMetrics(
        int WordsCompleted,
        int FirstAttemptCorrectCount,
        double AverageSimilarityScore,
        double AccuracyPercent,
        double AverageAttemptsPerWord,
        double FirstAttemptSuccessRate);

    public record CategoryMetrics(
        string Category,
        double AccuracyPercent,
        double AverageSimilarity,
        double AverageAttemptsPerWord,
        int WordsAttempted);

    public static class PatientAnalyticsEngine
    {
        public static SessionMetrics CalculateSessionMetrics(IReadOnlyList<ParsedSpeechAttempt> attempts)
        {
            if (attempts.Count == 0)
            {
                return new SessionMetrics(0, 0, 0, 0, 0, 0);
            }

            var correctCount = attempts.Count(a => a.IsCorrect);
            var accuracy = AnalyticsHelpers.Round((double)correctCount / attempts.Count * 100);

            var wordGroups = attempts
                .GroupBy(a => $"{a.VocabularyId}|{a.ExpectedWord}")
                .ToList();

            var wordsCompleted = wordGroups.Count;
            var firstAttemptCorrect = wordGroups.Count(g =>
                g.OrderBy(a => a.AttemptNumber).First().IsCorrect);

            var bestSimilarityPerWord = wordGroups
                .Select(g => g.Max(a => a.SimilarityScore))
                .ToList();

            var avgSimilarity = AnalyticsHelpers.Round(bestSimilarityPerWord.Average());
            var avgAttemptsPerWord = AnalyticsHelpers.Round(wordGroups.Average(g => g.Count()));
            var firstAttemptRate = wordsCompleted > 0
                ? AnalyticsHelpers.Round((double)firstAttemptCorrect / wordsCompleted * 100)
                : 0;

            return new SessionMetrics(
                wordsCompleted,
                firstAttemptCorrect,
                avgSimilarity,
                accuracy,
                avgAttemptsPerWord,
                firstAttemptRate);
        }

        public static List<CategoryMetrics> CalculateCategoryMetrics(IReadOnlyList<ParsedSpeechAttempt> attempts)
        {
            return attempts
                .GroupBy(a => AnalyticsHelpers.NormalizeCategory(a.Category))
                .Select(g =>
                {
                    var wordGroups = g.GroupBy(a => $"{a.VocabularyId}|{a.ExpectedWord}").ToList();
                    var correct = g.Count(a => a.IsCorrect);
                    var accuracy = g.Count() > 0
                        ? AnalyticsHelpers.Round((double)correct / g.Count() * 100)
                        : 0;

                    return new CategoryMetrics(
                        g.Key,
                        accuracy,
                        AnalyticsHelpers.Round(wordGroups.Select(wg => wg.Max(a => a.SimilarityScore)).DefaultIfEmpty(0).Average()),
                        AnalyticsHelpers.Round(wordGroups.Average(wg => wg.Count())),
                        wordGroups.Count);
                })
                .OrderByDescending(c => c.AccuracyPercent)
                .ToList();
        }

        public static double CalculateOverallImprovement(IEnumerable<(DateTime Date, double Accuracy)> points)
        {
            var list = points.OrderBy(p => p.Date).ToList();
            if (list.Count < 2) return 0;

            var firstWindow = list.Take(Math.Max(1, list.Count / 3)).Average(p => p.Accuracy);
            var lastWindow = list.TakeLast(Math.Max(1, list.Count / 3)).Average(p => p.Accuracy);
            return AnalyticsHelpers.Round(lastWindow - firstWindow);
        }
    }
}
