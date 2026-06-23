namespace Nutq.Core.Models
{
    public class PlanAnalyticsOptions
    {
        public double WsrWeight { get; set; } = 0.4;
        public double AvgSimilarityWeight { get; set; } = 0.3;
        public double FasrWeight { get; set; } = 0.3;
        public double FailedWordPenalty { get; set; } = 5.0;

        public double ExcellentThreshold { get; set; } = 85.0;
        public double GoodThreshold { get; set; } = 70.0;
        public double ModerateThreshold { get; set; } = 50.0;

        public double StrongImprovementThreshold { get; set; } = 15.0;
        public double ImprovingThreshold { get; set; } = 5.0;
        public double DecliningThreshold { get; set; } = -5.0;
        public double CriticalDeclineThreshold { get; set; } = -15.0;

        public double LowSimilarityWordThreshold { get; set; } = 60.0;
        public double HighRetryWordThreshold { get; set; } = 2.0;

        public string ActiveGenerator { get; set; } = "RuleBased"; // "RuleBased" or "AI"
    }
}
