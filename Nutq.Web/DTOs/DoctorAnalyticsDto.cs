namespace Nutq.Web.DTOs
{
    public class DoctorAnalyticsDto
    {
        public int TotalPatients { get; set; }
        public int TotalPlans { get; set; }
        public int TotalExercises { get; set; }
        public double AverageCompletionRate { get; set; }
    }

    public class TrendResultDto
    {
        public double Delta { get; set; }
        public string Direction { get; set; } = "InsufficientData";
    }

    public class CategoryScoreEntryDto
    {
        public string Category { get; set; } = string.Empty;
        public double AccuracyPercent { get; set; }
    }

    public class SessionTimelineEntryDto
    {
        public int TrainingSessionId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double OverallScore { get; set; }
        public IEnumerable<CategoryScoreEntryDto> CategoryScores { get; set; } = Array.Empty<CategoryScoreEntryDto>();
    }

    public class CategoryTrendEntryDto
    {
        public string Category { get; set; } = string.Empty;
        public double Delta { get; set; }
        public string Direction { get; set; } = "InsufficientData";
        public double FirstSessionScore { get; set; }
        public double LastSessionScore { get; set; }
    }

    public class PatientLongitudinalAnalyticsDto
    {
        public int PatientId { get; set; }
        public TrendResultDto OverallTrend { get; set; } = new();
        public IEnumerable<SessionTimelineEntryDto> SessionTimeline { get; set; } = Array.Empty<SessionTimelineEntryDto>();
        public IEnumerable<CategoryTrendEntryDto> CategoryTrends { get; set; } = Array.Empty<CategoryTrendEntryDto>();
    }
}
