using Microsoft.AspNetCore.Mvc;
using Nutq.Core.Interfaces;
using Nutq.Web.DTOs.PatientDashboard;

namespace Nutq.Web.Controllers
{
    [ApiController]
    [Route("api/patient-dashboard")]
    public class PatientDashboardController : ControllerBase
    {
        private readonly IPatientDashboardService _service;

        public PatientDashboardController(IPatientDashboardService service)
        {
            _service = service;
        }

        [HttpGet("{patientId}")]
        public async Task<IActionResult> GetDashboard(int patientId)
        {
            var plans = await _service.GetPatientPlansAsync(patientId);
            var progress = await _service.GetPatientProgressAsync(patientId);

            var result = plans.Select(plan => new PatientDashboardDto
            {
                PlanId = plan.Id,
                PlanName = plan.Description!,
                Exercises = plan.PlanExercises!.Select(pe =>
                {
                    var prog = progress.FirstOrDefault(p => p.PlanExerciseId == pe.Id);
                    return new ExerciseProgressDto
                    {
                        ExerciseId = pe.ExerciseId,
                        ExerciseName = pe.Exercise.Name,
                        Completed = prog?.Completed ?? false,
                        Score = prog?.Score
                    };
                }).ToList()
            });

            return Ok(result);
        }
    }
}
