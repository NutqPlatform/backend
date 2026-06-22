namespace Nutq.Core.Services
{
    public static class AnalyticsHelpers
    {
        public const double TrendThresholdPercent = 5.0;
        public const string AnalysisSourceDeterministic = "Deterministic";
        public const string AnalysisSourceAi = "Ai";

        public static string NormalizeCategory(string? category)
        {
            if (string.IsNullOrWhiteSpace(category)) return "uncategorized";
            return category.Trim().ToLowerInvariant();
        }

        public static double Round(double value, int decimals = 2)
            => Math.Round(value, decimals, MidpointRounding.AwayFromZero);

        public static string DetermineTrendDirection(double currentAccuracy, double? previousAccuracy)
        {
            if (!previousAccuracy.HasValue) return "InsufficientData";

            var delta = currentAccuracy - previousAccuracy.Value;
            if (delta >= TrendThresholdPercent) return "Improving";
            if (delta <= -TrendThresholdPercent) return "Declining";
            return "Stable";
        }

        public static string ClassifyPatternType(string expected, string recognized, double similarity)
        {
            if (string.IsNullOrWhiteSpace(recognized)) return "Omission";
            if (recognized.Length < expected.Length * 0.5) return "Omission";
            if (recognized.Length > expected.Length * 1.5) return "Addition";
            if (similarity >= 50 && similarity < 70) return "Distortion";
            return "Substitution";
        }

        public static double CalculateSeverityScore(double averageSimilarity, int occurrenceCount)
        {
            var similarityComponent = (100 - averageSimilarity) * 0.6;
            var frequencyComponent = Math.Min(occurrenceCount * 5, 40);
            return Round(Math.Min(100, similarityComponent + frequencyComponent));
        }

        public static DateTime GetPeriodStart(string period, DateTime reference)
        {
            return period.ToLowerInvariant() switch
            {
                "weekly" => reference.Date.AddDays(-(int)reference.DayOfWeek),
                "monthly" => new DateTime(reference.Year, reference.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                _ => reference.Date
            };
        }

        public static IEnumerable<IGrouping<DateTime, T>> GroupByPeriod<T>(
            IEnumerable<T> items,
            string period,
            Func<T, DateTime> dateSelector)
        {
            return period.ToLowerInvariant() switch
            {
                "weekly" => items.GroupBy(i => dateSelector(i).Date.AddDays(-(int)dateSelector(i).DayOfWeek)),
                "monthly" => items.GroupBy(i => new DateTime(dateSelector(i).Year, dateSelector(i).Month, 1)),
                _ => items.GroupBy(i => dateSelector(i).Date)
            };
        }
    }
}
