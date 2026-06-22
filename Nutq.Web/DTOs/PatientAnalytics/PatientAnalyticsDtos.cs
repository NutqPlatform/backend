namespace Nutq.Web.DTOs.PatientAnalytics
{
    public class PatientPerformanceSummaryDto
    {
        public int TotalSessions { get; set; }
        public int TotalTrainingTimeSeconds { get; set; }
        public double AccuracyPercent { get; set; }
        public double FirstAttemptSuccessRate { get; set; }
        public double AverageSimilarity { get; set; }
        public double AverageAttemptsPerWord { get; set; }
        public IEnumerable<string> StrongestCategories { get; set; } = Array.Empty<string>();
        public IEnumerable<string> WeakestCategories { get; set; } = Array.Empty<string>();
        public IEnumerable<string> RecommendedFocusAreas { get; set; } = Array.Empty<string>();
    }

    public class TrainingSessionSummaryDto
    {
        public int Id { get; set; }
        public int ExerciseProgressId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public string? ExerciseCategory { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TotalDurationSeconds { get; set; }
        public int WordsCompleted { get; set; }
        public int FirstAttemptCorrectCount { get; set; }
        public double AverageSimilarityScore { get; set; }
        public double AccuracyPercent { get; set; }
    }

    public class ProgressDataPointDto
    {
        public DateTime Date { get; set; }
        public double AccuracyPercent { get; set; }
        public double FirstAttemptSuccessRate { get; set; }
        public double AverageSimilarity { get; set; }
        public int CumulativeTrainingTimeSeconds { get; set; }
    }

    public class PatientProgressTrendsDto
    {
        public string Period { get; set; } = "daily";
        public IEnumerable<ProgressDataPointDto> DataPoints { get; set; } = Array.Empty<ProgressDataPointDto>();
        public double OverallImprovementPercent { get; set; }
    }

    public class CategoryPerformanceItemDto
    {
        public string Category { get; set; } = string.Empty;
        public double AccuracyPercent { get; set; }
        public double AverageSimilarity { get; set; }
        public double AverageAttemptsPerWord { get; set; }
        public int WordsAttempted { get; set; }
        public string TrendDirection { get; set; } = "InsufficientData";
        public double? PreviousAccuracyPercent { get; set; }
    }

    public class CategoryAnalysisResultDto
    {
        public IEnumerable<CategoryPerformanceItemDto> Categories { get; set; } = Array.Empty<CategoryPerformanceItemDto>();
        public IEnumerable<CategoryPerformanceItemDto> Strongest { get; set; } = Array.Empty<CategoryPerformanceItemDto>();
        public IEnumerable<CategoryPerformanceItemDto> Weakest { get; set; } = Array.Empty<CategoryPerformanceItemDto>();
        public IEnumerable<CategoryPerformanceItemDto> Improving { get; set; } = Array.Empty<CategoryPerformanceItemDto>();
        public IEnumerable<CategoryPerformanceItemDto> NeedsFocus { get; set; } = Array.Empty<CategoryPerformanceItemDto>();
    }

    public class ChartSeriesPointDto
    {
        public DateTime Date { get; set; }
        public double Value { get; set; }
    }

    public class CategoryTrendSeriesDto
    {
        public string Category { get; set; } = string.Empty;
        public IEnumerable<ChartSeriesPointDto> Points { get; set; } = Array.Empty<ChartSeriesPointDto>();
    }

    public class ChartDataResultDto
    {
        public IEnumerable<ChartSeriesPointDto> AccuracyOverTime { get; set; } = Array.Empty<ChartSeriesPointDto>();
        public IEnumerable<ChartSeriesPointDto> SimilarityOverTime { get; set; } = Array.Empty<ChartSeriesPointDto>();
        public IEnumerable<ChartSeriesPointDto> FirstAttemptSuccessOverTime { get; set; } = Array.Empty<ChartSeriesPointDto>();
        public IEnumerable<CategoryTrendSeriesDto> CategoryPerformanceOverTime { get; set; } = Array.Empty<CategoryTrendSeriesDto>();
        public IEnumerable<ChartSeriesPointDto> OverallImprovementTrend { get; set; } = Array.Empty<ChartSeriesPointDto>();
    }

    public class ClinicalReportSummaryDto
    {
        public int Id { get; set; }
        public int TrainingSessionId { get; set; }
        public DateTime GeneratedAt { get; set; }
        public int SessionDurationSeconds { get; set; }
        public int ExercisesCompleted { get; set; }
        public double AccuracyRate { get; set; }
        public double FirstAttemptSuccessRate { get; set; }
        public double AveragePronunciationSimilarity { get; set; }
    }

    public class RecommendedFocusItemDto
    {
        public string Category { get; set; } = string.Empty;
        public string Rationale { get; set; } = string.Empty;
        public int Priority { get; set; }
    }

    public class ClinicalReportDetailDto
    {
        public int Id { get; set; }
        public int TrainingSessionId { get; set; }
        public DateTime GeneratedAt { get; set; }
        public int SessionDurationSeconds { get; set; }
        public int ExercisesCompleted { get; set; }
        public double AccuracyRate { get; set; }
        public double FirstAttemptSuccessRate { get; set; }
        public double AveragePronunciationSimilarity { get; set; }
        public IEnumerable<string> StrengthAreas { get; set; } = Array.Empty<string>();
        public IEnumerable<string> WeaknessAreas { get; set; } = Array.Empty<string>();
        public IEnumerable<RecommendedFocusItemDto> RecommendedFocus { get; set; } = Array.Empty<RecommendedFocusItemDto>();
        public string AnalysisSource { get; set; } = "Deterministic";
    }

    public class PronunciationPatternDto
    {
        public int Id { get; set; }
        public string ExpectedPattern { get; set; } = string.Empty;
        public string RecognizedPattern { get; set; } = string.Empty;
        public string PatternType { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int OccurrenceCount { get; set; }
        public double AverageSimilarityScore { get; set; }
        public double SeverityScore { get; set; }
        public string AnalysisSource { get; set; } = "Deterministic";
        public DateTime LastDetectedAt { get; set; }
    }

    public class SpeechAttemptSummaryDto
    {
        public int AttemptNumber { get; set; }
        public string ExpectedWord { get; set; } = string.Empty;
        public string RecognizedWord { get; set; } = string.Empty;
        public double SimilarityScore { get; set; }
        public bool IsCorrect { get; set; }
        public double AudioDurationSeconds { get; set; }
        public DateTime AttemptedAt { get; set; }
    }

    public class WordSessionPerformanceDto
    {
        public int? VocabularyId { get; set; }
        public string ExpectedWord { get; set; } = string.Empty;
        public string WordEnglish { get; set; } = string.Empty;
        public string WordArabic { get; set; } = string.Empty;
        public int TotalAttempts { get; set; }
        public bool FirstAttemptCorrect { get; set; }
        public double BestSimilarityScore { get; set; }
        public double AverageSimilarityScore { get; set; }
        public bool Succeeded { get; set; }
        public IEnumerable<SpeechAttemptSummaryDto> Attempts { get; set; } = Array.Empty<SpeechAttemptSummaryDto>();
    }

    public class PatientExerciseSessionAnalyticsDto
    {
        public int TrainingSessionId { get; set; }
        public int ExerciseProgressId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TotalDurationSeconds { get; set; }
        public int WordsCompleted { get; set; }
        public int FirstAttemptCorrectCount { get; set; }
        public double AccuracyPercent { get; set; }
        public double FirstAttemptSuccessRate { get; set; }
        public double AverageSimilarityScore { get; set; }
        public IEnumerable<WordSessionPerformanceDto> Words { get; set; } = Array.Empty<WordSessionPerformanceDto>();
        public IEnumerable<string> StrengthAreas { get; set; } = Array.Empty<string>();
        public IEnumerable<string> WeaknessAreas { get; set; } = Array.Empty<string>();
    }
}
