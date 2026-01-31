using Microsoft.AspNetCore.Mvc;
using Nutq.Core.Interfaces;
using Nutq.Web.DTOs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nutq.Web.Controllers
{
    /// <summary>
    /// API endpoints for patients to view and interact with exercises in their therapy plans.
    /// </summary>
    [ApiController]
    [Route("api/patient-exercises")]
    public class PatientExerciseController : ControllerBase
    {
        private readonly ITherapyPlanService _therapyPlanService;
        private readonly IVocabularyRepository _vocabularyRepo;
        private readonly IExerciseProgressService _exerciseProgressService;
        private readonly IPlanExerciseRepository _planExerciseRepo;

        public PatientExerciseController(
            ITherapyPlanService therapyPlanService,
            IVocabularyRepository vocabularyRepo,
            IExerciseProgressService exerciseProgressService,
            IPlanExerciseRepository planExerciseRepo)
        {
            _therapyPlanService = therapyPlanService;
            _vocabularyRepo = vocabularyRepo;
            _exerciseProgressService = exerciseProgressService;
            _planExerciseRepo = planExerciseRepo;
        }

        /// <summary>
        /// Get all exercises assigned to a specific therapy plan. Only returns plans belonging to the patient.
        /// </summary>
        /// <param name="patientId">Patient ID</param>
        /// <param name="planId">Therapy plan ID</param>
        [HttpGet("{patientId}/plans/{planId}/exercises")]
        public async Task<IActionResult> GetPlanExercises(int patientId, int planId)
        {
            try
            {
                var plan = await _therapyPlanService.GetPlanWithExercisesForPatientAsync(planId, patientId);
                if (plan == null)
                    return NotFound(new { error = "Therapy plan not found or does not belong to this patient" });

                var dtos = (plan.PlanExercises ?? Enumerable.Empty<Nutq.Core.Entities.PlanExercise>())
                    .Select(pe => new PlanExerciseDto
                    {
                        Id = pe.Id,
                        TherapyPlanId = pe.TherapyPlanId,
                        ExerciseId = pe.ExerciseId,
                        DurationMinutes = pe.DurationMinutes,
                        Repetition = pe.Repetition,
                        AiConstraints = pe.AiConstraints,
                        Exercise = pe.Exercise != null ? new ExerciseDto
                        {
                            Id = pe.Exercise.Id,
                            Name = pe.Exercise.Name,
                            Description = pe.Exercise.Description,
                            Category = pe.Exercise.Category,
                            Difficulty = pe.Exercise.Difficulty,
                            DifficultyId = pe.Exercise.DifficultyId
                        } : null
                    }).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get vocabulary items for an exercise - only "Fruits" category and "Easy" difficulty level.
        /// Validates that the plan exercise belongs to a therapy plan assigned to the patient.
        /// </summary>
        /// <param name="patientId">Patient ID</param>
        /// <param name="planExerciseId">Plan exercise ID</param>
        [HttpGet("{patientId}/exercises/{planExerciseId}/vocabulary")]
        public async Task<IActionResult> GetExerciseVocabulary(int patientId, int planExerciseId)
        {
            try
            {
                var planExercise = await _planExerciseRepo.GetByIdAsync(planExerciseId);
                if (planExercise == null)
                    return NotFound(new { error = "Plan exercise not found" });

                var plan = await _therapyPlanService.GetPlanWithExercisesForPatientAsync(planExercise.TherapyPlanId, patientId);
                if (plan == null)
                    return NotFound(new { error = "Plan exercise does not belong to this patient" });

                var vocabularies = await _vocabularyRepo.GetByCategoryAndDifficultyLevelAsync("Fruits", "Easy");

                var dtos = vocabularies.Select(v => new VocabularyDto
                {
                    Id = v.Id,
                    WordArabic = v.WordArabic,
                    WordEnglish = v.WordEnglish,
                    Category = v.Category,
                    DifficultyLevelName = v.DifficultyLevel?.Name ?? v.DifficultyLevel?.Level,
                    ImageUrl = v.ImageUrl,
                    SoundUrl = v.SoundUrl,
                    VideoUrl = v.VideoUrl
                }).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Start an exercise - records StartTime. Creates an ExerciseProgress record.
        /// </summary>
        /// <param name="patientId">Patient ID</param>
        /// <param name="planExerciseId">Plan exercise ID</param>
        [HttpPost("{patientId}/exercises/{planExerciseId}/start")]
        public async Task<IActionResult> StartExercise(int patientId, int planExerciseId)
        {
            try
            {
                await _exerciseProgressService.StartExerciseAsync(patientId, planExerciseId);
                return Ok(new { message = "Exercise started successfully", startTime = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Complete an exercise - records EndTime, sets Completed = true, Score = null.
        /// </summary>
        /// <param name="patientId">Patient ID</param>
        /// <param name="planExerciseId">Plan exercise ID</param>
        [HttpPost("{patientId}/exercises/{planExerciseId}/complete")]
        public async Task<IActionResult> CompleteExercise(int patientId, int planExerciseId)
        {
            try
            {
                await _exerciseProgressService.CompleteExerciseAsync(patientId, planExerciseId);
                return Ok(new { message = "Exercise completed successfully", endTime = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
