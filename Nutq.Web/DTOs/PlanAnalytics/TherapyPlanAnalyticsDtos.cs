namespace Nutq.Web.DTOs.PlanAnalytics
{
    public class TherapyPlanSummaryDto
    {
        public int TotalSessionDurationSeconds { get; set; }
        public int TotalWordsPracticed { get; set; }
        public int TotalSpeechAttempts { get; set; }
        public double WordSuccessRate { get; set; }
        public double AttemptAccuracyRate { get; set; }
        public double FirstAttemptSuccessRate { get; set; }
        public double AveragePronunciationSimilarity { get; set; }
        public int TotalFailedWords { get; set; }
        public int TotalCompletedWords { get; set; }
        public int TotalSessions { get; set; }
    }

    public class RecognizedWordHistoryDto
    {
        public int AttemptNumber { get; set; }
        public string RecognizedWord { get; set; } = string.Empty;
        public double SimilarityScore { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class PlanWordPerformanceDto
    {
        public string Word { get; set; } = string.Empty;
        public string WordEnglish { get; set; } = string.Empty;
        public string WordArabic { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int TotalAttempts { get; set; }
        public double BestSimilarityScore { get; set; }
        public double AverageSimilarityScore { get; set; }
        public bool FirstAttemptSuccess { get; set; }
        public bool FinalSuccess { get; set; }
        public double TimeSpentSeconds { get; set; }
        public IEnumerable<RecognizedWordHistoryDto> RecognizedWordHistory { get; set; } = Array.Empty<RecognizedWordHistoryDto>();
    }

    public class PlanCategoryPerformanceDto
    {
        public string Category { get; set; } = string.Empty;
        public int WordsAttempted { get; set; }
        public int WordsSucceeded { get; set; }
        public double AccuracyPercent { get; set; }
        public double AverageSimilarity { get; set; }
        public double AverageAttemptsPerWord { get; set; }
    }

    public class PlanStrengthAnalysisDto
    {
        public IEnumerable<PlanWordPerformanceDto> BestPerformingWords { get; set; } = Array.Empty<PlanWordPerformanceDto>();
        public IEnumerable<PlanCategoryPerformanceDto> BestPerformingCategories { get; set; } = Array.Empty<PlanCategoryPerformanceDto>();
        public IEnumerable<PlanWordPerformanceDto> MasteredOnFirstAttempt { get; set; } = Array.Empty<PlanWordPerformanceDto>();
        public IEnumerable<string> ConsistentlyStrongAreas { get; set; } = Array.Empty<string>();
    }

    public class PlanWeaknessAnalysisDto
    {
        public IEnumerable<PlanWordPerformanceDto> FailedWords { get; set; } = Array.Empty<PlanWordPerformanceDto>();
        public IEnumerable<PlanWordPerformanceDto> HighRetryWords { get; set; } = Array.Empty<PlanWordPerformanceDto>();
        public IEnumerable<PlanCategoryPerformanceDto> LowPerformanceCategories { get; set; } = Array.Empty<PlanCategoryPerformanceDto>();
        public IEnumerable<PlanWordPerformanceDto> LowSimilarityWords { get; set; } = Array.Empty<PlanWordPerformanceDto>();
        public IEnumerable<string> RecurringDifficulties { get; set; } = Array.Empty<string>();
    }

    public class PlanPeriodComparisonDto
    {
        public string Period { get; set; } = string.Empty;
        public double? AccuracyDelta { get; set; }
        public double? SimilarityDelta { get; set; }
        public double? FirstAttemptDelta { get; set; }
        public bool HasData { get; set; }
    }

    public class PlanProgressComparisonDto
    {
        public PlanPeriodComparisonDto VsPreviousSession { get; set; } = new();
        public PlanPeriodComparisonDto VsPreviousPlan { get; set; } = new();
        public PlanPeriodComparisonDto VsLast7Days { get; set; } = new();
        public PlanPeriodComparisonDto VsLast30Days { get; set; } = new();
    }

    public class PlanFocusAreaItemDto
    {
        public string Area { get; set; } = string.Empty;
        public string Rationale { get; set; } = string.Empty;
        public int Priority { get; set; }
    }

    public class PlanClinicalInsightsDto
    {
        public IEnumerable<string> Strengths { get; set; } = Array.Empty<string>();
        public IEnumerable<string> Weaknesses { get; set; } = Array.Empty<string>();
        public IEnumerable<PlanFocusAreaItemDto> RecommendedFocusAreas { get; set; } = Array.Empty<PlanFocusAreaItemDto>();
        public IEnumerable<string> SuggestedNextExercises { get; set; } = Array.Empty<string>();
        public IEnumerable<string> TherapyAttentionAreas { get; set; } = Array.Empty<string>();

        /// <summary>
        /// "Deterministic" or "Ai" — signals whether insights were rule-based or AI-generated.
        /// Frontend renders identically regardless of value.
        /// </summary>
        public string AnalysisSource { get; set; } = "Deterministic";
    }

    public class TherapyPlanAnalyticsDto
    {
        public int PlanId { get; set; }
        public int PatientId { get; set; }
        public string PlanDescription { get; set; } = string.Empty;
        public string PlanStatus { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TherapyPlanSummaryDto Summary { get; set; } = new();
        public IEnumerable<PlanWordPerformanceDto> Words { get; set; } = Array.Empty<PlanWordPerformanceDto>();
        public IEnumerable<PlanCategoryPerformanceDto> Categories { get; set; } = Array.Empty<PlanCategoryPerformanceDto>();
        public PlanStrengthAnalysisDto Strengths { get; set; } = new();
        public PlanWeaknessAnalysisDto Weaknesses { get; set; } = new();
        public PlanProgressComparisonDto ProgressComparison { get; set; } = new();
        public PlanClinicalInsightsDto ClinicalInsights { get; set; } = new();
    }
}
