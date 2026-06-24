using System;
using System.Collections.Generic;

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
        public double MasteredSimilarity { get; set; }
        public double PlanOutcomeScore { get; set; }
        public string PlanOutcomeRating { get; set; } = string.Empty;
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
        public string TrendRating { get; set; } = "Stable";
    }

    public class PlanProgressComparisonDto
    {
        public PlanPeriodComparisonDto VsPreviousSession { get; set; } = new();
        public PlanPeriodComparisonDto VsPreviousPlan { get; set; } = new();
        public PlanProgressComparisonDto() { }
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
        public string ClinicalSummary { get; set; } = string.Empty;
        public IEnumerable<string> StrengthAnalysis { get; set; } = Array.Empty<string>();
        public IEnumerable<string> WeaknessAnalysis { get; set; } = Array.Empty<string>();
        public IEnumerable<string> TreatmentRecommendations { get; set; } = Array.Empty<string>();
        public IEnumerable<PlanFocusAreaItemDto> SuggestedFocusAreas { get; set; } = Array.Empty<PlanFocusAreaItemDto>();
        public IEnumerable<string> TherapistNotes { get; set; } = Array.Empty<string>();
        public string AnalysisSource { get; set; } = "RuleBased";
    }

    public class RecurringDifficultyItemDto
    {
        public string Word { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int Frequency { get; set; }
        public double SeverityScore { get; set; }
        public string AttentionLevel { get; set; } = "Low";
    }

    public class SuggestedNextTherapyContentDto
    {
        public IEnumerable<string> CategoriesNeedingReinforcement { get; set; } = Array.Empty<string>();
        public IEnumerable<string> VocabularyNeedingRepetition { get; set; } = Array.Empty<string>();
        public string DifficultyAdjustment { get; set; } = string.Empty;
        public int RecommendedExerciseCount { get; set; }
        public string Reasoning { get; set; } = string.Empty;
    }

    // ─── Session Timeline DTOs ─────────────────────────────────────────────────

    /// <summary>Per-word breakdown within a single training session.</summary>
    public class PlanSessionWordDto
    {
        public string ExpectedWord { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int TotalAttempts { get; set; }
        public double BestSimilarityScore { get; set; }
        public double AverageSimilarityScore { get; set; }
        public bool Succeeded { get; set; }
    }

    /// <summary>One training session inside a therapy plan.</summary>
    public class PlanSessionTimelineDto
    {
        public int SessionNumber { get; set; }
        public int TrainingSessionId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationSeconds { get; set; }
        public double AccuracyPercent { get; set; }
        public double AverageSimilarityScore { get; set; }
        public int TotalAttempts { get; set; }
        public int WordsSucceeded { get; set; }
        public int WordsAttempted { get; set; }
        public IEnumerable<PlanSessionWordDto> Words { get; set; } = Array.Empty<PlanSessionWordDto>();
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
        public IEnumerable<RecurringDifficultyItemDto> RecurringDifficulties { get; set; } = Array.Empty<RecurringDifficultyItemDto>();
        public SuggestedNextTherapyContentDto SuggestedNextContent { get; set; } = new();
        /// <summary>Ordered list of all training sessions for this plan, newest last.</summary>
        public IEnumerable<PlanSessionTimelineDto> SessionTimeline { get; set; } = Array.Empty<PlanSessionTimelineDto>();
    }


    // ─── Therapist PDF Report Model (Export-Ready structure) ──────────────────

    public class TherapyPlanReportModel
    {
        public int ReportId { get; set; }
        public string GeneratedAt { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string PatientAge { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        
        public int PlanId { get; set; }
        public string PlanDescription { get; set; } = string.Empty;
        public string PlanStatus { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        
        public TherapyPlanSummaryDto Summary { get; set; } = new();
        public IEnumerable<PlanWordPerformanceDto> Words { get; set; } = Array.Empty<PlanWordPerformanceDto>();
        public IEnumerable<PlanCategoryPerformanceDto> Categories { get; set; } = Array.Empty<PlanCategoryPerformanceDto>();
        public PlanProgressComparisonDto ProgressComparison { get; set; } = new();
        public PlanClinicalInsightsDto ClinicalInsights { get; set; } = new();
        public IEnumerable<RecurringDifficultyItemDto> RecurringDifficulties { get; set; } = Array.Empty<RecurringDifficultyItemDto>();
        public SuggestedNextTherapyContentDto SuggestedNextContent { get; set; } = new();
    }
}
