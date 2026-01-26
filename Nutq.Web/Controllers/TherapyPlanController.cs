using Microsoft.AspNetCore.Mvc;
using Nutq.Core.Interfaces;
using Nutq.Core.Commands;
using Nutq.Web.DTOs; 
namespace Nutq.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TherapyPlanController : ControllerBase
    {
        private readonly ITherapyPlanService _planService;
        private readonly IExerciseProgressRepository _progressRepo;

        public TherapyPlanController(ITherapyPlanService planService, IExerciseProgressRepository progressRepo)
        {
            _planService = planService;
            _progressRepo = progressRepo;
        }

        [HttpPost("doctor/{doctorId}/patients/{patientId}/plan")]
public async Task<IActionResult> CreatePlan(int doctorId, int patientId, [FromBody] CreateTherapyPlanCommand command)
{
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
    try
    {
        var planExercise = await _planService.AddExerciseToPlanAsync(planId, command);

        var exercise = planExercise.Exercise; 

        var dto = new PlanExerciseDto
        {
            Id = planExercise.Id,
            TherapyPlanId = planExercise.TherapyPlanId,
            ExerciseId = planExercise.ExerciseId,
            DurationMinutes = planExercise.DurationMinutes,
            Repetition = planExercise.Repetition,
            AiConstraints = planExercise.AiConstraints,
            Exercise = new ExerciseDto
            {
                Id = exercise.Id,
                Name = exercise.Name,
                Description = exercise.Description,
                Category = exercise.Category,
                Difficulty = exercise.Difficulty
            }
        };

        return Ok(dto);
    }
    catch (Exception ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}


       [HttpGet("doctor/{doctorId}/patients/{patientId}/plans")]
public async Task<IActionResult> GetPlans(int doctorId, int patientId)
{
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
    try
    {
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
    try
    {
        await _planService.UpdatePlanStatusAsync(planId, dto.Status);
        return Ok(new { success = true, message = "Plan status updated" });
    }
    catch (Exception ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}

[HttpGet("doctor/{doctorId}/plans/active")]
public async Task<IActionResult> GetActivePlans(int doctorId)
{
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
    try
    {
        var plan = await _planService.GetPlanByIdAsync(planId);
        if (plan == null)
            return NotFound(new { error = "Plan not found" });

        var planExerciseIds = plan.PlanExercises == null
            ? new List<int>()
            : plan.PlanExercises.Select(pe => pe.Id).ToList();

        var progresses = await _progressRepo.GetByPlanExerciseIdsAsync(planExerciseIds);

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

       }
}

public class UpdatePlanStatusDto
{
    public string Status { get; set; } = string.Empty;
}
