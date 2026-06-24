using Microsoft.AspNetCore.Mvc;
using Nutq.Core.Interfaces;
using Nutq.Core.Models;
using Nutq.Web.DTOs;
using Nutq.Web.DTOs.PlanAnalytics;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nutq.Web.Controllers
{
    [ApiController]
    [Route("api/doctor-analytics")]
    public class DoctorAnalyticsController : ControllerBase
    {
        private readonly IDoctorAnalyticsService _service;
        private readonly ITherapyPlanRepository _planRepo;

        public DoctorAnalyticsController(IDoctorAnalyticsService service, ITherapyPlanRepository planRepo)
        {
            _service = service;
            _planRepo = planRepo;
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

        [HttpGet("/api/doctors/{doctorId}/plans/{planId}/analytics/pdf-model")]
        public async Task<IActionResult> GetPlanAnalyticsPdfModel(int doctorId, int planId)
        {
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null || user.Value.Role != "doctor" || user.Value.UserId != doctorId)
                return Forbid();

            var plan = await _planRepo.GetPlanWithExercisesByIdAsync(planId);
            if (plan == null || plan.DoctorId != doctorId)
                return NotFound(new { error = "Plan not found." });

            var analytics = await _service.GetTherapyPlanAnalyticsAsync(doctorId, planId);
            if (analytics == null)
                return NotFound(new { error = "Analytics not found." });

            int? age = null;
            if (plan.Patient?.DateOfBirth != null)
            {
                age = DateTime.Today.Year - plan.Patient.DateOfBirth.Value.Year;
                if (plan.Patient.DateOfBirth.Value.Date > DateTime.Today.AddYears(-age.Value)) age--;
            }

            var pdfModel = new TherapyPlanReportModel
            {
                ReportId = new Random().Next(10000, 99999),
                GeneratedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"),
                DoctorName = plan.Doctor?.Name ?? $"Doctor #{doctorId}",
                PatientName = plan.Patient?.Name ?? $"Patient #{plan.PatientId}",
                PatientAge = age?.ToString() ?? "N/A",
                Diagnosis = plan.Patient?.DiagnosisText ?? "N/A",
                PlanId = planId,
                PlanDescription = plan.Description ?? "Untitled Plan",
                PlanStatus = plan.Status ?? "Unknown",
                StartDate = plan.StartDate.ToString("yyyy-MM-dd"),
                EndDate = plan.EndDate?.ToString("yyyy-MM-dd") ?? "N/A",
                Summary = MapSummaryDto(analytics.Summary),
                Words = analytics.Words.Select(MapWordPerformance),
                Categories = analytics.Categories.Select(MapCategoryPerformance),
                ProgressComparison = MapProgressComparisonDto(analytics.ProgressComparison),
                ClinicalInsights = MapClinicalInsightsDto(analytics.ClinicalInsights),
                RecurringDifficulties = analytics.RecurringDifficulties.Select(MapRecurringDifficulty),
                SuggestedNextContent = MapSuggestedNextContent(analytics.SuggestedNextContent)
            };

            return Ok(pdfModel);
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
                Summary = MapSummaryDto(a.Summary),
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
                ProgressComparison = MapProgressComparisonDto(a.ProgressComparison),
                ClinicalInsights = MapClinicalInsightsDto(a.ClinicalInsights),
                RecurringDifficulties = a.RecurringDifficulties.Select(MapRecurringDifficulty),
                SuggestedNextContent = MapSuggestedNextContent(a.SuggestedNextContent),
                SessionTimeline = a.SessionTimeline.Select((s, idx) => new PlanSessionTimelineDto
                {
                    SessionNumber = s.SessionNumber,
                    TrainingSessionId = s.TrainingSessionId,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    DurationSeconds = s.DurationSeconds,
                    AccuracyPercent = s.AccuracyPercent,
                    AverageSimilarityScore = s.AverageSimilarityScore,
                    TotalAttempts = s.TotalAttempts,
                    WordsSucceeded = s.WordsSucceeded,
                    WordsAttempted = s.WordsAttempted,
                    Words = s.Words.Select(w => new PlanSessionWordDto
                    {
                        ExpectedWord = w.ExpectedWord,
                        Category = w.Category,
                        TotalAttempts = w.TotalAttempts,
                        BestSimilarityScore = w.BestSimilarityScore,
                        AverageSimilarityScore = w.AverageSimilarityScore,
                        Succeeded = w.Succeeded
                    })
                })
            };


        private static TherapyPlanSummaryDto MapSummaryDto(TherapyPlanSummary s) =>
            new()
            {
                TotalSessionDurationSeconds = s.TotalSessionDurationSeconds,
                TotalWordsPracticed = s.TotalWordsPracticed,
                TotalSpeechAttempts = s.TotalSpeechAttempts,
                WordSuccessRate = s.WordSuccessRate,
                AttemptAccuracyRate = s.AttemptAccuracyRate,
                FirstAttemptSuccessRate = s.FirstAttemptSuccessRate,
                AveragePronunciationSimilarity = s.AveragePronunciationSimilarity,
                TotalFailedWords = s.TotalFailedWords,
                TotalCompletedWords = s.TotalCompletedWords,
                TotalSessions = s.TotalSessions,
                MasteredSimilarity = s.MasteredSimilarity,
                PlanOutcomeScore = s.PlanOutcomeScore,
                PlanOutcomeRating = s.PlanOutcomeRating
            };

        private static PlanPeriodComparisonDto MapPeriodComparison(PlanPeriodComparison p) =>
            new()
            {
                Period = p.Period,
                AccuracyDelta = p.AccuracyDelta,
                SimilarityDelta = p.SimilarityDelta,
                FirstAttemptDelta = p.FirstAttemptDelta,
                HasData = p.HasData,
                TrendRating = p.TrendRating
            };

        private static PlanProgressComparisonDto MapProgressComparisonDto(PlanProgressComparison pc) =>
            new()
            {
                VsPreviousSession = MapPeriodComparison(pc.VsPreviousSession),
                VsPreviousPlan = MapPeriodComparison(pc.VsPreviousPlan),
                VsLast7Days = MapPeriodComparison(pc.VsLast7Days),
                VsLast30Days = MapPeriodComparison(pc.VsLast30Days)
            };

        private static PlanClinicalInsightsDto MapClinicalInsightsDto(PlanClinicalInsights ci) =>
            new()
            {
                ClinicalSummary = ci.ClinicalSummary,
                StrengthAnalysis = ci.StrengthAnalysis,
                WeaknessAnalysis = ci.WeaknessAnalysis,
                TreatmentRecommendations = ci.TreatmentRecommendations,
                SuggestedFocusAreas = ci.SuggestedFocusAreas.Select(f =>
                    new PlanFocusAreaItemDto { Area = f.Area, Rationale = f.Rationale, Priority = f.Priority }),
                TherapistNotes = ci.TherapistNotes,
                AnalysisSource = ci.AnalysisSource
            };

        private static RecurringDifficultyItemDto MapRecurringDifficulty(RecurringDifficultyItem r) =>
            new()
            {
                Word = r.Word,
                Category = r.Category,
                Frequency = r.Frequency,
                SeverityScore = r.SeverityScore,
                AttentionLevel = r.AttentionLevel
            };

        private static SuggestedNextTherapyContentDto MapSuggestedNextContent(SuggestedNextTherapyContent sc) =>
            new()
            {
                CategoriesNeedingReinforcement = sc.CategoriesNeedingReinforcement,
                VocabularyNeedingRepetition = sc.VocabularyNeedingRepetition,
                DifficultyAdjustment = sc.DifficultyAdjustment,
                RecommendedExerciseCount = sc.RecommendedExerciseCount,
                Reasoning = sc.Reasoning
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
    }
}
