using Microsoft.AspNetCore.Mvc;
using Nutq.Core.Interfaces;
using Nutq.Core.Models;
using Nutq.Web.DTOs;
using Nutq.Web.DTOs.PlanAnalytics;
using System.Threading.Tasks;

namespace Nutq.Web.Controllers
{
    [ApiController]
    [Route("api/doctor-analytics")]
    public class DoctorAnalyticsController : ControllerBase
    {
        private readonly IDoctorAnalyticsService _service;

        public DoctorAnalyticsController(IDoctorAnalyticsService service)
        {
            _service = service;
        }

        [HttpGet("{doctorId}")]
        public async Task<IActionResult> GetAnalytics(int doctorId)
        {
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null || user.Value.Role != "doctor" || user.Value.UserId != doctorId)
                return Forbid();

            var dto = new DoctorAnalyticsDto
            {
                TotalPatients = await _service.GetTotalPatientsAsync(doctorId),
                TotalPlans = await _service.GetTotalPlansAsync(doctorId),
                TotalExercises = await _service.GetTotalExercisesAsync(doctorId),
                AverageCompletionRate = await _service.GetAverageCompletionRateAsync(doctorId)
            };

            return Ok(dto);
        }

        [HttpGet("/api/doctors/{doctorId}/patients/{patientId}/analytics")]
        public async Task<IActionResult> GetPatientLongitudinalAnalytics(int doctorId, int patientId)
        {
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null || user.Value.Role != "doctor" || user.Value.UserId != doctorId)
                return Forbid();

            var analytics = await _service.GetPatientLongitudinalAnalyticsAsync(doctorId, patientId);
            if (analytics == null)
                return Forbid();

            return Ok(MapLongitudinalAnalytics(analytics));
        }

        [HttpGet("/api/doctors/{doctorId}/plans/{planId}/analytics")]
        public async Task<IActionResult> GetTherapyPlanAnalytics(int doctorId, int planId)
        {
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null || user.Value.Role != "doctor" || user.Value.UserId != doctorId)
                return Forbid();

            var analytics = await _service.GetTherapyPlanAnalyticsAsync(doctorId, planId);
            if (analytics == null)
                return NotFound(new { error = "Plan not found or access denied." });

            return Ok(MapPlanAnalytics(analytics));
        }

        // ─── Mapping helpers ─────────────────────────────────────────────────────

        private static PatientLongitudinalAnalyticsDto MapLongitudinalAnalytics(PatientLongitudinalAnalytics analytics) =>
            new()
            {
                PatientId = analytics.PatientId,
                OverallTrend = new TrendResultDto
                {
                    Delta = analytics.OverallTrend.Delta,
                    Direction = analytics.OverallTrend.Direction
                },
                SessionTimeline = analytics.SessionTimeline.Select(session => new SessionTimelineEntryDto
                {
                    TrainingSessionId = session.TrainingSessionId,
                    StartTime = session.StartTime,
                    EndTime = session.EndTime,
                    OverallScore = session.OverallScore,
                    CategoryScores = session.CategoryScores.Select(category => new CategoryScoreEntryDto
                    {
                        Category = category.Category,
                        AccuracyPercent = category.AccuracyPercent
                    })
                }),
                CategoryTrends = analytics.CategoryTrends.Select(category => new CategoryTrendEntryDto
                {
                    Category = category.Category,
                    Delta = category.Delta,
                    Direction = category.Direction,
                    FirstSessionScore = category.FirstSessionScore,
                    LastSessionScore = category.LastSessionScore
                })
            };

        private static TherapyPlanAnalyticsDto MapPlanAnalytics(TherapyPlanAnalytics a) =>
            new()
            {
                PlanId = a.PlanId,
                PatientId = a.PatientId,
                PlanDescription = a.PlanDescription,
                PlanStatus = a.PlanStatus,
                StartDate = a.StartDate,
                EndDate = a.EndDate,
                Summary = new TherapyPlanSummaryDto
                {
                    TotalSessionDurationSeconds = a.Summary.TotalSessionDurationSeconds,
                    TotalWordsPracticed = a.Summary.TotalWordsPracticed,
                    TotalSpeechAttempts = a.Summary.TotalSpeechAttempts,
                    WordSuccessRate = a.Summary.WordSuccessRate,
                    AttemptAccuracyRate = a.Summary.AttemptAccuracyRate,
                    FirstAttemptSuccessRate = a.Summary.FirstAttemptSuccessRate,
                    AveragePronunciationSimilarity = a.Summary.AveragePronunciationSimilarity,
                    TotalFailedWords = a.Summary.TotalFailedWords,
                    TotalCompletedWords = a.Summary.TotalCompletedWords,
                    TotalSessions = a.Summary.TotalSessions
                },
                Words = a.Words.Select(MapWordPerformance),
                Categories = a.Categories.Select(MapCategoryPerformance),
                Strengths = new PlanStrengthAnalysisDto
                {
                    BestPerformingWords = a.Strengths.BestPerformingWords.Select(MapWordPerformance),
                    BestPerformingCategories = a.Strengths.BestPerformingCategories.Select(MapCategoryPerformance),
                    MasteredOnFirstAttempt = a.Strengths.MasteredOnFirstAttempt.Select(MapWordPerformance),
                    ConsistentlyStrongAreas = a.Strengths.ConsistentlyStrongAreas
                },
                Weaknesses = new PlanWeaknessAnalysisDto
                {
                    FailedWords = a.Weaknesses.FailedWords.Select(MapWordPerformance),
                    HighRetryWords = a.Weaknesses.HighRetryWords.Select(MapWordPerformance),
                    LowPerformanceCategories = a.Weaknesses.LowPerformanceCategories.Select(MapCategoryPerformance),
                    LowSimilarityWords = a.Weaknesses.LowSimilarityWords.Select(MapWordPerformance),
                    RecurringDifficulties = a.Weaknesses.RecurringDifficulties
                },
                ProgressComparison = new PlanProgressComparisonDto
                {
                    VsPreviousSession = MapPeriodComparison(a.ProgressComparison.VsPreviousSession),
                    VsPreviousPlan = MapPeriodComparison(a.ProgressComparison.VsPreviousPlan),
                    VsLast7Days = MapPeriodComparison(a.ProgressComparison.VsLast7Days),
                    VsLast30Days = MapPeriodComparison(a.ProgressComparison.VsLast30Days)
                },
                ClinicalInsights = new PlanClinicalInsightsDto
                {
                    Strengths = a.ClinicalInsights.Strengths,
                    Weaknesses = a.ClinicalInsights.Weaknesses,
                    RecommendedFocusAreas = a.ClinicalInsights.RecommendedFocusAreas.Select(f =>
                        new PlanFocusAreaItemDto { Area = f.Area, Rationale = f.Rationale, Priority = f.Priority }),
                    SuggestedNextExercises = a.ClinicalInsights.SuggestedNextExercises,
                    TherapyAttentionAreas = a.ClinicalInsights.TherapyAttentionAreas,
                    AnalysisSource = a.ClinicalInsights.AnalysisSource
                }
            };

        private static PlanWordPerformanceDto MapWordPerformance(PlanWordPerformance w) =>
            new()
            {
                Word = w.Word,
                WordEnglish = w.WordEnglish,
                WordArabic = w.WordArabic,
                Category = w.Category,
                TotalAttempts = w.TotalAttempts,
                BestSimilarityScore = w.BestSimilarityScore,
                AverageSimilarityScore = w.AverageSimilarityScore,
                FirstAttemptSuccess = w.FirstAttemptSuccess,
                FinalSuccess = w.FinalSuccess,
                TimeSpentSeconds = w.TimeSpentSeconds,
                RecognizedWordHistory = w.RecognizedWordHistory.Select(h => new RecognizedWordHistoryDto
                {
                    AttemptNumber = h.AttemptNumber,
                    RecognizedWord = h.RecognizedWord,
                    SimilarityScore = h.SimilarityScore,
                    IsCorrect = h.IsCorrect
                })
            };

        private static PlanCategoryPerformanceDto MapCategoryPerformance(PlanCategoryPerformance c) =>
            new()
            {
                Category = c.Category,
                WordsAttempted = c.WordsAttempted,
                WordsSucceeded = c.WordsSucceeded,
                AccuracyPercent = c.AccuracyPercent,
                AverageSimilarity = c.AverageSimilarity,
                AverageAttemptsPerWord = c.AverageAttemptsPerWord
            };

        private static PlanPeriodComparisonDto MapPeriodComparison(PlanPeriodComparison p) =>
            new()
            {
                Period = p.Period,
                AccuracyDelta = p.AccuracyDelta,
                SimilarityDelta = p.SimilarityDelta,
                FirstAttemptDelta = p.FirstAttemptDelta,
                HasData = p.HasData
            };
    }
}
