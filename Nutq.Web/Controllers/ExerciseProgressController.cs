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
        private readonly IDoctorPatientRelationshipRepository _relationshipRepo;

        public ExerciseProgressController(
            IExerciseProgressService progressService,
            IDoctorPatientRelationshipRepository relationshipRepo)
        {
            _progressService = progressService;
            _relationshipRepo = relationshipRepo;
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdateProgress([FromBody] ExerciseProgressCommand command)
        {
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null || user.Value.Role != "patient" || user.Value.UserId != command.PatientId)
                return Forbid();

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
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null)
                return Forbid();

            if (user.Value.Role == "patient")
            {
                if (user.Value.UserId != patientId)
                    return Forbid();
            }
            else if (user.Value.Role == "doctor")
            {
                if (!await _relationshipRepo.HasRelationshipAsync(user.Value.UserId, patientId))
                    return Forbid();
            }
            else
            {
                return Forbid();
            }

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
                ExerciseName = p.PlanExercise?.Exercise?.Name ?? "Unknown",
                CurrentRepetition = p.CurrentRepetition,
                TotalRepetitions = p.TotalRepetitions,
                SessionData = p.SessionData
            }).ToList();

            return Ok(dtos);
        }

        [HttpPost("{patientId}/{planExerciseId}/start")]
        public async Task<IActionResult> StartExercise(int patientId, int planExerciseId)
        {
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null || user.Value.Role != "patient" || user.Value.UserId != patientId)
                return Forbid();

            try
            {
                await _progressService.StartExerciseAsync(patientId, planExerciseId);
                return Ok(new { message = "Exercise started", startTime = System.DateTime.UtcNow });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{patientId}/{planExerciseId}/complete-repetition")]
        public async Task<IActionResult> CompleteRepetition(int patientId, int planExerciseId)
        {
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null || user.Value.Role != "patient" || user.Value.UserId != patientId)
                return Forbid();

            try
            {
                await _progressService.CompleteRepetitionAsync(patientId, planExerciseId);
                return Ok(new { message = "Repetition completed" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{patientId}/{planExerciseId}/complete")]
        public async Task<IActionResult> CompleteExercise(int patientId, int planExerciseId)
        {
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null || user.Value.Role != "patient" || user.Value.UserId != patientId)
                return Forbid();

            try
            {
                await _progressService.CompleteExerciseAsync(patientId, planExerciseId);
                return Ok(new { message = "Exercise completed", endTime = System.DateTime.UtcNow });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
