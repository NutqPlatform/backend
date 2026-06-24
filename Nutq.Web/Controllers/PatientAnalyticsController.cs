using Microsoft.AspNetCore.Mvc;
using Nutq.Core.Interfaces;
using Nutq.Web.DTOs.PatientAnalytics;

namespace Nutq.Web.Controllers
{
    [ApiController]
    [Route("api/patient-analytics")]
    public class PatientAnalyticsController : ControllerBase
    {
        private readonly IPatientAnalyticsService _analyticsService;
        private readonly IPronunciationPatternRepository _patternRepo;

        public PatientAnalyticsController(
            IPatientAnalyticsService analyticsService,
            IPronunciationPatternRepository patternRepo)
        {
            _analyticsService = analyticsService;
            _patternRepo = patternRepo;
        }

        [HttpGet("{patientId}/summary")]
        public async Task<IActionResult> GetSummary(int patientId, [FromQuery] int doctorId)
        {
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null || user.Value.Role != "doctor" || user.Value.UserId != doctorId)
                return Forbid();

            if (!await _analyticsService.CanDoctorAccessPatientAsync(doctorId, patientId))
                return Forbid();

            var summary = await _analyticsService.GetSummaryAsync(patientId);
            return Ok(new PatientPerformanceSummaryDto
            {
                TotalSessions = summary.TotalSessions,
                TotalTrainingTimeSeconds = summary.TotalTrainingTimeSeconds,
                AccuracyPercent = summary.AccuracyPercent,
                FirstAttemptSuccessRate = summary.FirstAttemptSuccessRate,
                AverageSimilarity = summary.AverageSimilarity,
                AverageAttemptsPerWord = summary.AverageAttemptsPerWord,
                StrongestCategories = summary.StrongestCategories,
                WeakestCategories = summary.WeakestCategories,
                RecommendedFocusAreas = summary.RecommendedFocusAreas
            });
        }

        [HttpGet("{patientId}/sessions")]
        public async Task<IActionResult> GetSessions(
            int patientId,
            [FromQuery] int doctorId,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null || user.Value.Role != "doctor" || user.Value.UserId != doctorId)
                return Forbid();

            if (!await _analyticsService.CanDoctorAccessPatientAsync(doctorId, patientId))
                return Forbid();

            var sessions = await _analyticsService.GetSessionsAsync(patientId, doctorId, from, to);
            return Ok(sessions.Select(s => new TrainingSessionSummaryDto
            {
                Id = s.Id,
                ExerciseProgressId = s.ExerciseProgressId,
                ExerciseName = s.ExerciseName,
                ExerciseCategory = s.ExerciseCategory,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                TotalDurationSeconds = s.TotalDurationSeconds,
                WordsCompleted = s.WordsCompleted,
                FirstAttemptCorrectCount = s.FirstAttemptCorrectCount,
                AverageSimilarityScore = s.AverageSimilarityScore,
                AccuracyPercent = s.AccuracyPercent
            }));
        }

        [HttpGet("{patientId}/progress")]
        public async Task<IActionResult> GetProgress(
            int patientId,
            [FromQuery] int doctorId,
            [FromQuery] string period = "daily")
        {
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null || user.Value.Role != "doctor" || user.Value.UserId != doctorId)
                return Forbid();

            if (!await _analyticsService.CanDoctorAccessPatientAsync(doctorId, patientId))
                return Forbid();

            var progress = await _analyticsService.GetProgressAsync(patientId, period);
            return Ok(new PatientProgressTrendsDto
            {
                Period = progress.Period,
                OverallImprovementPercent = progress.OverallImprovementPercent,
                DataPoints = progress.DataPoints.Select(d => new ProgressDataPointDto
                {
                    Date = d.Date,
                    AccuracyPercent = d.AccuracyPercent,
                    FirstAttemptSuccessRate = d.FirstAttemptSuccessRate,
                    AverageSimilarity = d.AverageSimilarity,
                    CumulativeTrainingTimeSeconds = d.CumulativeTrainingTimeSeconds
                })
            });
        }

        [HttpGet("{patientId}/categories")]
        public async Task<IActionResult> GetCategories(int patientId, [FromQuery] int doctorId)
        {
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null || user.Value.Role != "doctor" || user.Value.UserId != doctorId)
                return Forbid();

            if (!await _analyticsService.CanDoctorAccessPatientAsync(doctorId, patientId))
                return Forbid();

            var analysis = await _analyticsService.GetCategoryAnalysisAsync(patientId);
            return Ok(MapCategoryAnalysis(analysis));
        }

        [HttpGet("{patientId}/chart-data")]
        public async Task<IActionResult> GetChartData(
            int patientId,
            [FromQuery] int doctorId,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null || user.Value.Role != "doctor" || user.Value.UserId != doctorId)
                return Forbid();

            if (!await _analyticsService.CanDoctorAccessPatientAsync(doctorId, patientId))
                return Forbid();

            var charts = await _analyticsService.GetChartDataAsync(patientId, from, to);
            return Ok(new ChartDataResultDto
            {
                AccuracyOverTime = MapPoints(charts.AccuracyOverTime),
                SimilarityOverTime = MapPoints(charts.SimilarityOverTime),
                FirstAttemptSuccessOverTime = MapPoints(charts.FirstAttemptSuccessOverTime),
                CategoryPerformanceOverTime = charts.CategoryPerformanceOverTime.Select(c => new CategoryTrendSeriesDto
                {
                    Category = c.Category,
                    Points = MapPoints(c.Points)
                }),
                OverallImprovementTrend = MapPoints(charts.OverallImprovementTrend)
            });
        }

        [HttpGet("{patientId}/reports")]
        public async Task<IActionResult> GetReports(
            int patientId,
            [FromQuery] int doctorId,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null || user.Value.Role != "doctor" || user.Value.UserId != doctorId)
                return Forbid();

            if (!await _analyticsService.CanDoctorAccessPatientAsync(doctorId, patientId))
                return Forbid();

            var reports = await _analyticsService.GetReportsAsync(patientId, doctorId, from, to);
            return Ok(reports.Select(r => new ClinicalReportSummaryDto
            {
                Id = r.Id,
                TrainingSessionId = r.TrainingSessionId,
                GeneratedAt = r.GeneratedAt,
                SessionDurationSeconds = r.SessionDurationSeconds,
                ExercisesCompleted = r.ExercisesCompleted,
                AccuracyRate = r.AccuracyRate,
                FirstAttemptSuccessRate = r.FirstAttemptSuccessRate,
                AveragePronunciationSimilarity = r.AveragePronunciationSimilarity
            }));
        }

        [HttpGet("{patientId}/reports/{sessionId}")]
        public async Task<IActionResult> GetReport(int patientId, int sessionId, [FromQuery] int doctorId)
        {
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null || user.Value.Role != "doctor" || user.Value.UserId != doctorId)
                return Forbid();

            if (!await _analyticsService.CanDoctorAccessPatientAsync(doctorId, patientId))
                return Forbid();

            var report = await _analyticsService.GetReportAsync(patientId, sessionId, doctorId);
            if (report == null) return NotFound(new { error = "Report not found" });

            return Ok(new ClinicalReportDetailDto
            {
                Id = report.Id,
                TrainingSessionId = report.TrainingSessionId,
                GeneratedAt = report.GeneratedAt,
                SessionDurationSeconds = report.SessionDurationSeconds,
                ExercisesCompleted = report.ExercisesCompleted,
                AccuracyRate = report.AccuracyRate,
                FirstAttemptSuccessRate = report.FirstAttemptSuccessRate,
                AveragePronunciationSimilarity = report.AveragePronunciationSimilarity,
                StrengthAreas = report.StrengthAreas,
                WeaknessAreas = report.WeaknessAreas,
                RecommendedFocus = report.RecommendedFocus.Select(r => new RecommendedFocusItemDto
                {
                    Category = r.Category,
                    Rationale = r.Rationale,
                    Priority = r.Priority
                }),
                AnalysisSource = report.AnalysisSource
            });
        }

        [HttpGet("{patientId}/pronunciation-patterns")]
        public async Task<IActionResult> GetPronunciationPatterns(
            int patientId,
            [FromQuery] int doctorId,
            [FromQuery] bool activeOnly = true)
        {
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null || user.Value.Role != "doctor" || user.Value.UserId != doctorId)
                return Forbid();

            if (!await _analyticsService.CanDoctorAccessPatientAsync(doctorId, patientId))
                return Forbid();

            var patterns = activeOnly
                ? await _patternRepo.GetActiveByPatientAsync(patientId)
                : await _patternRepo.GetByPatientAsync(patientId);

            return Ok(patterns.Select(p => new PronunciationPatternDto
            {
                Id = p.Id,
                ExpectedPattern = p.ExpectedPattern,
                RecognizedPattern = p.RecognizedPattern,
                PatternType = p.PatternType,
                Category = p.Category,
                OccurrenceCount = p.OccurrenceCount,
                AverageSimilarityScore = p.AverageSimilarityScore,
                SeverityScore = p.SeverityScore,
                AnalysisSource = p.AnalysisSource,
                LastDetectedAt = p.LastDetectedAt
            }));
        }

        private static CategoryAnalysisResultDto MapCategoryAnalysis(CategoryAnalysisResult analysis) =>
            new()
            {
                Categories = MapCategoryItems(analysis.Categories),
                Strongest = MapCategoryItems(analysis.Strongest),
                Weakest = MapCategoryItems(analysis.Weakest),
                Improving = MapCategoryItems(analysis.Improving),
                NeedsFocus = MapCategoryItems(analysis.NeedsFocus)
            };

        private static IEnumerable<CategoryPerformanceItemDto> MapCategoryItems(IEnumerable<CategoryPerformanceItem> items) =>
            items.Select(i => new CategoryPerformanceItemDto
            {
                Category = i.Category,
                AccuracyPercent = i.AccuracyPercent,
                AverageSimilarity = i.AverageSimilarity,
                AverageAttemptsPerWord = i.AverageAttemptsPerWord,
                WordsAttempted = i.WordsAttempted,
                TrendDirection = i.TrendDirection,
                PreviousAccuracyPercent = i.PreviousAccuracyPercent
            });

        private static IEnumerable<ChartSeriesPointDto> MapPoints(IEnumerable<ChartSeriesPoint> points) =>
            points.Select(p => new ChartSeriesPointDto { Date = p.Date, Value = p.Value });
    }
}
