// Web/Controllers/ExerciseProgressController.cs
using Microsoft.AspNetCore.Mvc;
using Nutq.Core.Commands;
using Nutq.Core.Interfaces;
using Nutq.Web.DTOs;
using System.Linq;
using System.Threading.Tasks;

namespace Nutq.Web.Controllers
{
    [ApiController]
    [Route("api/exercise-progress")]
    public class ExerciseProgressController : ControllerBase
    {
        private readonly IExerciseProgressService _progressService;

        public ExerciseProgressController(IExerciseProgressService progressService)
        {
            _progressService = progressService;
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdateProgress([FromBody] ExerciseProgressCommand command)
        {
            try
            {
                await _progressService.AddOrUpdateProgressAsync(command);
                return Ok(new { message = "Progress recorded successfully" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("patient/{patientId}")]
        public async Task<IActionResult> GetPatientProgress(int patientId)
        {
            var progresses = await _progressService.GetPatientProgressAsync(patientId);

            var dtos = progresses.Select(p => new ExerciseProgressDto
            {
                Id = p.Id,
                PatientId = p.PatientId,
                PlanExerciseId = p.PlanExerciseId,
                StartTime = p.StartTime,
                EndTime = p.EndTime,
                Score = p.Score,
                Completed = p.Completed,
                ExerciseName = p.PlanExercise?.Exercise?.Name ?? "Unknown"
            }).ToList();

            return Ok(dtos);
        }
    }
}
