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

        public TherapyPlanController(ITherapyPlanService planService)
        {
            _planService = planService;
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

        var exercise = planExercise.Exercise; // لازم يكون مشمول بالـ Include في الريبو

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

       }
}
