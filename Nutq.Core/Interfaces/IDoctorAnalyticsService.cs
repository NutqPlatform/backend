using Nutq.Core.Models;

namespace Nutq.Core.Interfaces
{
    public interface IDoctorAnalyticsService
    {
        Task<int> GetTotalPatientsAsync(int doctorId);
        Task<int> GetTotalPlansAsync(int doctorId);
        Task<int> GetTotalExercisesAsync(int doctorId);
        Task<double> GetAverageCompletionRateAsync(int doctorId);
        Task<PatientLongitudinalAnalytics?> GetPatientLongitudinalAnalyticsAsync(int doctorId, int patientId);
        Task<TherapyPlanAnalytics?> GetTherapyPlanAnalyticsAsync(int doctorId, int planId);
    }

    // ─── Plan-level analytics domain records ────────────────────────────────────

    public record TherapyPlanAnalytics(
        int PlanId,
        int PatientId,
        string PlanDescription,
        string PlanStatus,
        DateTime StartDate,
        DateTime? EndDate,
        TherapyPlanSummary Summary,
        IReadOnlyList<PlanWordPerformance> Words,
        IReadOnlyList<PlanCategoryPerformance> Categories,
        PlanStrengthAnalysis Strengths,
        PlanWeaknessAnalysis Weaknesses,
        PlanProgressComparison ProgressComparison,
        PlanClinicalInsights ClinicalInsights,
        IReadOnlyList<RecurringDifficultyItem> RecurringDifficulties,
        SuggestedNextTherapyContent SuggestedNextContent);

    public record TherapyPlanSummary(
        int TotalSessionDurationSeconds,
        int TotalWordsPracticed,
        int TotalSpeechAttempts,
        double WordSuccessRate,
        double AttemptAccuracyRate,
        double FirstAttemptSuccessRate,
        double AveragePronunciationSimilarity,
        int TotalFailedWords,
        int TotalCompletedWords,
        int TotalSessions,
        double MasteredSimilarity,
        double PlanOutcomeScore,
        string PlanOutcomeRating);

    public record PlanWordPerformance(
        string Word,
        string WordEnglish,
        string WordArabic,
        string? Category,
        int TotalAttempts,
        double BestSimilarityScore,
        double AverageSimilarityScore,
        bool FirstAttemptSuccess,
        bool FinalSuccess,
        double TimeSpentSeconds,
        IReadOnlyList<RecognizedWordEntry> RecognizedWordHistory);

    public record RecognizedWordEntry(
        int AttemptNumber,
        string RecognizedWord,
        double SimilarityScore,
        bool IsCorrect);

    public record PlanCategoryPerformance(
        string Category,
        int WordsAttempted,
        int WordsSucceeded,
        double AccuracyPercent,
        double AverageSimilarity,
        double AverageAttemptsPerWord);

    public record PlanStrengthAnalysis(
        IReadOnlyList<PlanWordPerformance> BestPerformingWords,
        IReadOnlyList<PlanCategoryPerformance> BestPerformingCategories,
        IReadOnlyList<PlanWordPerformance> MasteredOnFirstAttempt,
        IReadOnlyList<string> ConsistentlyStrongAreas);

    public record PlanWeaknessAnalysis(
        IReadOnlyList<PlanWordPerformance> FailedWords,
        IReadOnlyList<PlanWordPerformance> HighRetryWords,
        IReadOnlyList<PlanCategoryPerformance> LowPerformanceCategories,
        IReadOnlyList<PlanWordPerformance> LowSimilarityWords,
        IReadOnlyList<string> RecurringDifficulties);

    public record PlanPeriodComparison(
        string Period,
        double? AccuracyDelta,
        double? SimilarityDelta,
        double? FirstAttemptDelta,
        bool HasData,
        string TrendRating);

    public record PlanProgressComparison(
        PlanPeriodComparison VsPreviousSession,
        PlanPeriodComparison VsPreviousPlan,
        PlanPeriodComparison VsLast7Days,
        PlanPeriodComparison VsLast30Days);

    public record PlanClinicalInsights(
        string ClinicalSummary,
        IReadOnlyList<string> StrengthAnalysis,
        IReadOnlyList<string> WeaknessAnalysis,
        IReadOnlyList<string> TreatmentRecommendations,
        IReadOnlyList<PlanFocusAreaItem> SuggestedFocusAreas,
        IReadOnlyList<string> TherapistNotes,
        string AnalysisSource);

    public record PlanFocusAreaItem(
        string Area,
        string Rationale,
        int Priority);

    public record RecurringDifficultyItem(
        string Word,
        string? Category,
        int Frequency,
        double SeverityScore,
        string AttentionLevel);

    public record SuggestedNextTherapyContent(
        IReadOnlyList<string> CategoriesNeedingReinforcement,
        IReadOnlyList<string> VocabularyNeedingRepetition,
        string DifficultyAdjustment,
        int RecommendedExerciseCount,
        string Reasoning);
}
