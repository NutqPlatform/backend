namespace Nutq.Core.Models
{
    public record PatientSessionTimelineProjection(
        int TrainingSessionId,
        DateTime StartTime,
        DateTime EndTime,
        double OverallScore,
        int? ProgressSnapshotId);

    public record PatientCategoryScoreProjection(
        int ProgressSnapshotId,
        int TrainingSessionId,
        string Category,
        double AccuracyPercent);

    public record TrendResult(double Delta, string Direction);

    public record CategoryScoreEntry(string Category, double AccuracyPercent);

    public record SessionTimelineEntry(
        int TrainingSessionId,
        DateTime StartTime,
        DateTime EndTime,
        double OverallScore,
        IReadOnlyList<CategoryScoreEntry> CategoryScores);

    public record CategoryTrendEntry(
        string Category,
        double Delta,
        string Direction,
        double FirstSessionScore,
        double LastSessionScore);

    public record PatientLongitudinalAnalytics(
        int PatientId,
        TrendResult OverallTrend,
        IReadOnlyList<SessionTimelineEntry> SessionTimeline,
        IReadOnlyList<CategoryTrendEntry> CategoryTrends);
}
