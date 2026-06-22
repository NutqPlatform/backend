using Microsoft.AspNetCore.Mvc;
using Nutq.Core.Interfaces;
using Nutq.Core.Models;
using Nutq.Web.DTOs;

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
            var analytics = await _service.GetPatientLongitudinalAnalyticsAsync(doctorId, patientId);
            if (analytics == null)
                return Forbid();

            return Ok(MapLongitudinalAnalytics(analytics));
        }

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
    }
}
