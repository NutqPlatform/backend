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

            var result = plans.Select(plan =>
            {
                var planExercises = plan.PlanExercises ?? new List<Nutq.Core.Entities.PlanExercise>();
                var exercisesDto = planExercises.Select(pe =>
                {
                    var prog = progress.FirstOrDefault(p => p.PlanExerciseId == pe.Id);
                    return new ExerciseProgressDto
                    {
                        ExerciseId = pe.ExerciseId,
                        ExerciseName = pe.Exercise.Name,
                        Completed = prog?.Completed ?? false,
                        Score = prog?.Score
                    };
                }).ToList();
                var completedCount = exercisesDto.Count(e => e.Completed);
                var totalCount = exercisesDto.Count;
                var progressPercentage = totalCount > 0 ? (double)completedCount / totalCount * 100 : 0;

                return new PatientDashboardDto
                {
                    PlanId = plan.Id,
                    PlanName = plan.Description ?? "Untitled Plan",
                    PlanStatus = plan.Status,
                    ProgressPercentage = progressPercentage,
                    Exercises = exercisesDto
                };
            });

            return Ok(result);
        }
    }
}
