using Microsoft.AspNetCore.Mvc;
using Nutq.Core.Interfaces;
using Nutq.Web.DTOs.PatientDashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nutq.Web.Controllers
{
    [ApiController]
    [Route("api/patient-dashboard")]
    public class PatientDashboardController : ControllerBase
    {
        private readonly IPatientDashboardService _service;
        private readonly IDoctorPatientRelationshipRepository _relationshipRepo;

        public PatientDashboardController(
            IPatientDashboardService service,
            IDoctorPatientRelationshipRepository relationshipRepo)
        {
            _service = service;
            _relationshipRepo = relationshipRepo;
        }

        [HttpGet("{patientId}")]
        public async Task<IActionResult> GetDashboard(int patientId)
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

            try
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
                            PlanExerciseId = pe.Id,
                            ExerciseId = pe.ExerciseId,
                            ExerciseName = pe.Exercise?.Name ?? "Unknown Exercise",
                            Completed = prog?.Completed ?? false,
                            Score = prog?.Score,
                            Started = prog != null,
                            CurrentRepetition = prog?.CurrentRepetition ?? 1,
                            TotalRepetitions = pe.Repetition
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
                        IsArchived = plan.IsArchived,
                        ProgressPercentage = progressPercentage,
                        Exercises = exercisesDto
                    };
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
