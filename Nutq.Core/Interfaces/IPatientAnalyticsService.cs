using Nutq.Core.Entities;

namespace Nutq.Core.Interfaces
{
    public interface IPatientAnalyticsIngestionService
    {
        Task IngestCompletedSessionAsync(ExerciseProgress progress);
    }

    public interface IPatientAnalyticsService
    {
        Task<bool> CanDoctorAccessPatientAsync(int doctorId, int patientId);
        Task<PatientPerformanceSummary> GetSummaryAsync(int patientId);
        Task<IEnumerable<TrainingSessionSummary>> GetSessionsAsync(int patientId, int? doctorId = null, DateTime? from = null, DateTime? to = null);
        Task<PatientProgressTrends> GetProgressAsync(int patientId, string period);
        Task<CategoryAnalysisResult> GetCategoryAnalysisAsync(int patientId);
        Task<ChartDataResult> GetChartDataAsync(int patientId, DateTime? from = null, DateTime? to = null);
        Task<IEnumerable<ClinicalReportSummary>> GetReportsAsync(int patientId, int? doctorId = null, DateTime? from = null, DateTime? to = null);
        Task<ClinicalReportDetail?> GetReportAsync(int patientId, int sessionId, int? doctorId = null);
        Task<PatientExerciseSessionAnalytics?> GetExerciseSessionAnalyticsAsync(int patientId, int planExerciseId);
    }

    public record SpeechAttemptSummary(
        int AttemptNumber,
        string ExpectedWord,
        string RecognizedWord,
        double SimilarityScore,
        bool IsCorrect,
        double AudioDurationSeconds,
        DateTime AttemptedAt);

    public record WordSessionPerformance(
        int? VocabularyId,
        string ExpectedWord,
        string WordEnglish,
        string WordArabic,
        int TotalAttempts,
        bool FirstAttemptCorrect,
        double BestSimilarityScore,
        double AverageSimilarityScore,
        bool Succeeded,
        IEnumerable<SpeechAttemptSummary> Attempts);

    public record PatientExerciseSessionAnalytics(
        int TrainingSessionId,
        int ExerciseProgressId,
        string ExerciseName,
        DateTime StartTime,
        DateTime EndTime,
        int TotalDurationSeconds,
        int WordsCompleted,
        int FirstAttemptCorrectCount,
        double AccuracyPercent,
        double FirstAttemptSuccessRate,
        double AverageSimilarityScore,
        IEnumerable<WordSessionPerformance> Words,
        IEnumerable<string> StrengthAreas,
        IEnumerable<string> WeaknessAreas);

    public record PatientPerformanceSummary(
        int TotalSessions,
        int TotalTrainingTimeSeconds,
        double AccuracyPercent,
        double FirstAttemptSuccessRate,
        double AverageSimilarity,
        double AverageAttemptsPerWord,
        IEnumerable<string> StrongestCategories,
        IEnumerable<string> WeakestCategories,
        IEnumerable<string> RecommendedFocusAreas);

    public record TrainingSessionSummary(
        int Id,
        int ExerciseProgressId,
        string ExerciseName,
        string? ExerciseCategory,
        DateTime StartTime,
        DateTime EndTime,
        int TotalDurationSeconds,
        int WordsCompleted,
        int FirstAttemptCorrectCount,
        double AverageSimilarityScore,
        double AccuracyPercent);

    public record ProgressDataPoint(
        DateTime Date,
        double AccuracyPercent,
        double FirstAttemptSuccessRate,
        double AverageSimilarity,
        int CumulativeTrainingTimeSeconds);

    public record PatientProgressTrends(
        string Period,
        IEnumerable<ProgressDataPoint> DataPoints,
        double OverallImprovementPercent);

    public record CategoryPerformanceItem(
        string Category,
        double AccuracyPercent,
        double AverageSimilarity,
        double AverageAttemptsPerWord,
        int WordsAttempted,
        string TrendDirection,
        double? PreviousAccuracyPercent);

    public record CategoryAnalysisResult(
        IEnumerable<CategoryPerformanceItem> Categories,
        IEnumerable<CategoryPerformanceItem> Strongest,
        IEnumerable<CategoryPerformanceItem> Weakest,
        IEnumerable<CategoryPerformanceItem> Improving,
        IEnumerable<CategoryPerformanceItem> NeedsFocus);

    public record ChartSeriesPoint(DateTime Date, double Value);

    public record ChartDataResult(
        IEnumerable<ChartSeriesPoint> AccuracyOverTime,
        IEnumerable<ChartSeriesPoint> SimilarityOverTime,
        IEnumerable<ChartSeriesPoint> FirstAttemptSuccessOverTime,
        IEnumerable<CategoryTrendSeries> CategoryPerformanceOverTime,
        IEnumerable<ChartSeriesPoint> OverallImprovementTrend);

    public record CategoryTrendSeries(string Category, IEnumerable<ChartSeriesPoint> Points);

    public record ClinicalReportSummary(
        int Id,
        int TrainingSessionId,
        DateTime GeneratedAt,
        int SessionDurationSeconds,
        int ExercisesCompleted,
        double AccuracyRate,
        double FirstAttemptSuccessRate,
        double AveragePronunciationSimilarity);

    public record ClinicalReportDetail(
        int Id,
        int TrainingSessionId,
        DateTime GeneratedAt,
        int SessionDurationSeconds,
        int ExercisesCompleted,
        double AccuracyRate,
        double FirstAttemptSuccessRate,
        double AveragePronunciationSimilarity,
        IEnumerable<string> StrengthAreas,
        IEnumerable<string> WeaknessAreas,
        IEnumerable<RecommendedFocusItem> RecommendedFocus,
        string AnalysisSource);

    public record RecommendedFocusItem(string Category, string Rationale, int Priority);
}
