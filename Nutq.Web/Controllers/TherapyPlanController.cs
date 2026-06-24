using Microsoft.AspNetCore.Mvc;
using Nutq.Core.Interfaces;
using Nutq.Core.Commands;
using Nutq.Core.Entities;
using Nutq.Web.DTOs; 
namespace Nutq.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TherapyPlanController : ControllerBase
    {
        private readonly ITherapyPlanService _planService;
        private readonly IExerciseProgressRepository _progressRepo;
        private readonly IPlanExerciseRepository _planExerciseRepo;
        private readonly IDoctorPatientRelationshipRepository _relationshipRepo;

        public TherapyPlanController(
            ITherapyPlanService planService, 
            IExerciseProgressRepository progressRepo, 
            IPlanExerciseRepository planExerciseRepo,
            IDoctorPatientRelationshipRepository relationshipRepo)
        {
            _planService = planService;
            _progressRepo = progressRepo;
            _planExerciseRepo = planExerciseRepo;
            _relationshipRepo = relationshipRepo;
        }

        private int? GetCurrentDoctorId()
        {
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null || user.Value.Role != "doctor")
                return null;
            return user.Value.UserId;
        }

        private (int UserId, string Role)? GetCurrentUser()
        {
            return JwtAuthorizationHelper.GetCurrentUser(Request);
        }

        [HttpPost("doctor/{doctorId}/patients/{patientId}/plan")]
        public async Task<IActionResult> CreatePlan(int doctorId, int patientId, [FromBody] CreateTherapyPlanCommand command)
        {
            var loggedInDoctorId = GetCurrentDoctorId();
            if (loggedInDoctorId == null || loggedInDoctorId != doctorId)
                return Forbid();

            try
            {
                var plan = await _planService.CreatePlanAsync(doctorId, patientId, command);

                var dto = new TherapyPlanDto
                {
                    Id = plan.Id,
                    Description = plan.Description,
                    Status = plan.Status,
                    StartDate = plan.StartDate,
                    EndDate = plan.EndDate
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("plan/{planId}/add-exercise")]
        public async Task<IActionResult> AddExercise(int planId, [FromBody] AddPlanExerciseCommand command)
        {
            var loggedInDoctorId = GetCurrentDoctorId();
            if (loggedInDoctorId == null)
                return Forbid();

            try
            {
                var plan = await _planService.GetPlanByIdAsync(planId);
                if (plan == null)
                    return NotFound(new { error = "Therapy plan not found" });

                if (plan.DoctorId != loggedInDoctorId)
                    return Forbid();

                var planExercises = await _planService.AddExerciseToPlanAsync(planId, command);

                var dtos = planExercises.Select(planExercise =>
                {
                    var exercise = planExercise.Exercise;
                    return new PlanExerciseDto
                    {
                        Id = planExercise.Id,
                        TherapyPlanId = planExercise.TherapyPlanId,
                        ExerciseId = planExercise.ExerciseId,
                        DurationMinutes = planExercise.DurationMinutes,
                        Repetition = planExercise.Repetition,
                        AiConstraints = planExercise.AiConstraints,
                        Exercise = exercise == null ? null : new ExerciseDto
                        {
                            Id = exercise.Id,
                            Name = exercise.Name,
                            Description = exercise.Description,
                            Category = exercise.Category,
                            Difficulty = exercise.Difficulty,
                            ImageUrl = exercise.ImageUrl,
                            AssetUrl = exercise.AssetUrl
                        }
                    };
                }).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("doctor/{doctorId}/patients/{patientId}/plans")]
        public async Task<IActionResult> GetPlans(int doctorId, int patientId)
        {
            var loggedInDoctorId = GetCurrentDoctorId();
            if (loggedInDoctorId == null || loggedInDoctorId != doctorId)
                return Forbid();

            var plans = await _planService.GetPlansForPatientAsync(doctorId, patientId);

            var dtos = plans.Select(plan => new TherapyPlanDto
            {
                Id = plan.Id,
                Description = plan.Description,
                Status = plan.Status,
                StartDate = plan.StartDate,
                EndDate = plan.EndDate,
                Exercises = plan.PlanExercises?.Select(pe => new PlanExerciseDto
                {
                    Id = pe.Id,
                    TherapyPlanId = pe.TherapyPlanId,
                    ExerciseId = pe.ExerciseId,
                    DurationMinutes = pe.DurationMinutes,
                    Repetition = pe.Repetition,
                    AiConstraints = pe.AiConstraints,
                    Exercise = new ExerciseDto
                    {
                        Id = pe.Exercise.Id,
                        Name = pe.Exercise.Name,
                        Description = pe.Exercise.Description,
                        Category = pe.Exercise.Category,
                        Difficulty = pe.Exercise.Difficulty
                    }
                }).ToList() ?? new List<PlanExerciseDto>()
            }).ToList();

            return Ok(dtos);
        }

        [HttpDelete("plan/{planId}/exercise/{planExerciseId}")]
        public async Task<IActionResult> DeleteExercise(int planId, int planExerciseId)
        {
            var loggedInDoctorId = GetCurrentDoctorId();
            if (loggedInDoctorId == null)
                return Forbid();

            try
            {
                var plan = await _planService.GetPlanByIdAsync(planId);
                if (plan == null)
                    return NotFound(new { error = "Therapy plan not found" });

                if (plan.DoctorId != loggedInDoctorId)
                    return Forbid();

                await _planService.DeleteExerciseFromPlanAsync(planId, planExerciseId);
                return Ok(new { success = true, message = "Exercise removed from plan" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("plan/{planId}/status")]
        public async Task<IActionResult> UpdateStatus(int planId, [FromBody] UpdatePlanStatusDto dto)
        {
            var loggedInDoctorId = GetCurrentDoctorId();
            if (loggedInDoctorId == null)
                return Forbid();

            try
            {
                var plan = await _planService.GetPlanByIdAsync(planId);
                if (plan == null)
                    return NotFound(new { error = "Therapy plan not found" });

                if (plan.DoctorId != loggedInDoctorId)
                    return Forbid();

                await _planService.UpdatePlanStatusAsync(planId, dto.Status);
                return Ok(new { success = true, message = "Plan status updated" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("plan/{planId}")]
        public async Task<IActionResult> UpdatePlan(int planId, [FromBody] UpdateTherapyPlanCommand command)
        {
            var loggedInDoctorId = GetCurrentDoctorId();
            if (loggedInDoctorId == null)
                return Forbid();

            try
            {
                var planObj = await _planService.GetPlanByIdAsync(planId);
                if (planObj == null)
                    return NotFound(new { error = "Therapy plan not found" });

                if (planObj.DoctorId != loggedInDoctorId)
                    return Forbid();

                var plan = await _planService.UpdatePlanAsync(planId, command);
                return Ok(new TherapyPlanDto
                {
                    Id = plan.Id,
                    Description = plan.Description,
                    Status = plan.Status,
                    StartDate = plan.StartDate,
                    EndDate = plan.EndDate
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("doctor/{doctorId}/plans/active")]
        public async Task<IActionResult> GetActivePlans(int doctorId)
        {
            var loggedInDoctorId = GetCurrentDoctorId();
            if (loggedInDoctorId == null || loggedInDoctorId != doctorId)
                return Forbid();

            var plans = await _planService.GetActivePlansForDoctorAsync(doctorId);

            var dtos = plans.Select(plan => new TherapyPlanDto
            {
                Id = plan.Id,
                Description = plan.Description,
                Status = plan.Status,
                StartDate = plan.StartDate,
                EndDate = plan.EndDate,
                PatientId = plan.PatientId,
                PatientName = plan.Patient?.Name,
                Exercises = plan.PlanExercises == null
                    ? new List<PlanExerciseDto>()
                    : plan.PlanExercises.Select(pe => new PlanExerciseDto
                    {
                        Id = pe.Id,
                        TherapyPlanId = pe.TherapyPlanId,
                        ExerciseId = pe.ExerciseId,
                        DurationMinutes = pe.DurationMinutes,
                        Repetition = pe.Repetition,
                        AiConstraints = pe.AiConstraints,
                        Exercise = new ExerciseDto
                        {
                            Id = pe.Exercise.Id,
                            Name = pe.Exercise.Name,
                            Description = pe.Exercise.Description,
                            Category = pe.Exercise.Category,
                            Difficulty = pe.Exercise.Difficulty
                        }
                    }).ToList()
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("plan/{planId}/progress")]
        public async Task<IActionResult> GetPlanProgress(int planId)
        {
            var user = GetCurrentUser();
            if (user == null)
                return Forbid();

            try
            {
                var plan = await _planService.GetPlanByIdAsync(planId);
                if (plan == null)
                    return NotFound(new { error = "Plan not found" });

                if (user.Value.Role == "doctor")
                {
                    // Authorization: only the plan owner can access plan progress.
                    // HasRelationshipAsync would allow any doctor who ever had the patient —
                    // including Doctor B after transfer — to see Doctor A's plan progress.
                    if (plan.DoctorId != user.Value.UserId)
                        return Forbid();
                }
                else if (user.Value.Role == "patient")
                {
                    if (plan.PatientId != user.Value.UserId)
                        return Forbid();
                }
                else
                {
                    return Forbid();
                }

                // Get plan exercises with progress data
                var planExercises = await _planExerciseRepo.GetByPlanIdsAsync(new List<int> { planId }) ?? new List<PlanExercise>();
                var planExerciseIds = planExercises.Select(pe => pe.Id).ToList();

                var progresses = planExerciseIds.Any()
                    ? await _progressRepo.GetByPlanExerciseIdsAsync(planExerciseIds)
                    : new List<ExerciseProgress>();

                var completedExercises = progresses
                    .Where(p => p.Completed)
                    .Select(p => p.PlanExerciseId)
                    .Distinct()
                    .Count();

                var totalExercises = planExerciseIds.Count;

                var progressPercentage = totalExercises > 0
                    ? (double)completedExercises / totalExercises * 100
                    : 0;

                return Ok(new
                {
                    progressPercentage,
                    completedExercises,
                    totalExercises
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("doctor/{doctorId}/ongoing-plans")]
        public async Task<IActionResult> GetOngoingPlans(int doctorId)
        {
            var loggedInDoctorId = GetCurrentDoctorId();
            if (loggedInDoctorId == null || loggedInDoctorId != doctorId)
                return Forbid();

            try
            {
                var plans = await _planService.GetOngoingPlansForDoctorAsync(doctorId);

                var dtos = new List<TherapyPlanDto>();
                foreach (var plan in plans)
                {
                    var planExerciseIds = plan.PlanExercises == null
                        ? new List<int>()
                        : plan.PlanExercises.Select(pe => pe.Id).ToList();

                    var progresses = planExerciseIds.Any()
                        ? await _progressRepo.GetByPlanExerciseIdsAsync(planExerciseIds)
                        : new List<ExerciseProgress>();

                    var completedExercises = progresses
                        .Where(p => p.Completed)
                        .Select(p => p.PlanExerciseId)
                        .Distinct()
                        .Count();

                    var totalExercises = planExerciseIds.Count;

                    var progressPercentage = totalExercises > 0
                        ? (double)completedExercises / totalExercises * 100
                        : 0;

                    var dto = new TherapyPlanDto
                    {
                        Id = plan.Id,
                        Description = plan.Description,
                        Status = plan.Status,
                        StartDate = plan.StartDate,
                        EndDate = plan.EndDate,
                        PatientId = plan.PatientId,
                        PatientName = plan.Patient?.Name,
                        ProgressPercentage = progressPercentage,
                        Exercises = plan.PlanExercises == null
                            ? new List<PlanExerciseDto>()
                            : plan.PlanExercises.Select(pe => new PlanExerciseDto
                            {
                                Id = pe.Id,
                                TherapyPlanId = pe.TherapyPlanId,
                                ExerciseId = pe.ExerciseId,
                                DurationMinutes = pe.DurationMinutes,
                                Repetition = pe.Repetition,
                                AiConstraints = pe.AiConstraints,
                                Exercise = new ExerciseDto
                                {
                                    Id = pe.Exercise.Id,
                                    Name = pe.Exercise.Name,
                                    Description = pe.Exercise.Description,
                                    Category = pe.Exercise.Category,
                                    Difficulty = pe.Exercise.Difficulty
                                }
                            }).ToList()
                    };
                    dtos.Add(dto);
                }

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("doctor/{doctorId}/plans/all")]
        public async Task<IActionResult> GetAllPlansForDoctor(int doctorId)
        {
            var loggedInDoctorId = GetCurrentDoctorId();
            if (loggedInDoctorId == null || loggedInDoctorId != doctorId)
                return Forbid();

            try
            {
                var plans = await _planService.GetPlansByDoctorAsync(doctorId);

                var dtos = new List<TherapyPlanDto>();
                foreach (var plan in plans)
                {
                    var planExerciseIds = plan.PlanExercises == null
                        ? new List<int>()
                        : plan.PlanExercises.Select(pe => pe.Id).ToList();

                    var progresses = planExerciseIds.Any()
                        ? await _progressRepo.GetByPlanExerciseIdsAsync(planExerciseIds)
                        : new List<ExerciseProgress>();

                    var completedExercises = progresses
                        .Where(p => p.Completed)
                        .Select(p => p.PlanExerciseId)
                        .Distinct()
                        .Count();

                    var totalExercises = planExerciseIds.Count;

                    var progressPercentage = totalExercises > 0
                        ? (double)completedExercises / totalExercises * 100
                        : 0;

                    var dto = new TherapyPlanDto
                    {
                        Id = plan.Id,
                        Description = plan.Description,
                        Status = plan.Status,
                        StartDate = plan.StartDate,
                        EndDate = plan.EndDate,
                        PatientId = plan.PatientId,
                        PatientName = plan.Patient?.Name,
                        ProgressPercentage = progressPercentage,
                        Exercises = plan.PlanExercises == null
                            ? new List<PlanExerciseDto>()
                            : plan.PlanExercises.Select(pe => new PlanExerciseDto
                            {
                                Id = pe.Id,
                                TherapyPlanId = pe.TherapyPlanId,
                                ExerciseId = pe.ExerciseId,
                                DurationMinutes = pe.DurationMinutes,
                                Repetition = pe.Repetition,
                                AiConstraints = pe.AiConstraints,
                                Exercise = new ExerciseDto
                                {
                                    Id = pe.Exercise.Id,
                                    Name = pe.Exercise.Name,
                                    Description = pe.Exercise.Description,
                                    Category = pe.Exercise.Category,
                                    Difficulty = pe.Exercise.Difficulty
                                }
                            }).ToList()
                    };

                    dtos.Add(dto);
                }

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class UpdatePlanStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}
