using Microsoft.AspNetCore.Mvc;
using Nutq.Core.Interfaces;
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
    }
}
