using Microsoft.AspNetCore.Mvc;
using Nutq.Core.Interfaces;
using Nutq.Web.DTOs;
using Nutq.Web.DTOs.PatientAnalytics;
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
        private readonly IVocabularyExerciseRepository _vocabExerciseRepo;
        private readonly IExerciseProgressService _exerciseProgressService;
        private readonly IPlanExerciseRepository _planExerciseRepo;
        private readonly IPatientAnalyticsService _analyticsService;
        private readonly IDoctorPatientRelationshipRepository _relationshipRepo;

        public PatientExerciseController(
            ITherapyPlanService therapyPlanService,
            IVocabularyRepository vocabularyRepo,
            IVocabularyExerciseRepository vocabExerciseRepo,
            IExerciseProgressService exerciseProgressService,
            IPlanExerciseRepository planExerciseRepo,
            IPatientAnalyticsService analyticsService,
            IDoctorPatientRelationshipRepository relationshipRepo)
        {
            _therapyPlanService = therapyPlanService;
            _vocabularyRepo = vocabularyRepo;
            _vocabExerciseRepo = vocabExerciseRepo;
            _exerciseProgressService = exerciseProgressService;
            _planExerciseRepo = planExerciseRepo;
            _analyticsService = analyticsService;
            _relationshipRepo = relationshipRepo;
        }

        

        /// <summary>
        /// Get all exercises assigned to a specific therapy plan. Only returns plans belonging to the patient.
        /// </summary>
        /// <param name="patientId">Patient ID</param>
        /// <param name="planId">Therapy plan ID</param>
        [HttpGet("{patientId}/plans/{planId}/exercises")]
        public async Task<IActionResult> GetPlanExercises(int patientId, int planId)
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
                // Plan owner (plan.DoctorId) always retains access regardless of relationship status.
                // A doctor with an active relationship but who is NOT the plan owner can also view
                // the exercises (they are managing the patient currently).
                // A doctor who previously had the patient but is NOT the plan owner cannot access.
                var planForAuth = await _therapyPlanService.GetPlanByIdAsync(planId);
                if (planForAuth == null)
                    return NotFound(new { error = "Therapy plan not found" });

                bool isPlanOwner = planForAuth.DoctorId == user.Value.UserId;
                bool hasActiveRelationship = await _relationshipRepo.HasRelationshipAsync(user.Value.UserId, patientId);

                if (!isPlanOwner && !hasActiveRelationship)
                    return Forbid();
            }
            else
            {
                return Forbid();
            }
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
                // Plan owner (plan.DoctorId) always retains access regardless of relationship status.
                // A doctor with an active relationship but who is NOT the plan owner can also view
                // the vocabulary (they are managing the patient currently).
                var planExerciseForAuth = await _planExerciseRepo.GetByIdAsync(planExerciseId);
                if (planExerciseForAuth != null)
                {
                    var planForAuth = await _therapyPlanService.GetPlanByIdAsync(planExerciseForAuth.TherapyPlanId);
                    bool isPlanOwner = planForAuth?.DoctorId == user.Value.UserId;
                    bool hasActiveRelationship = await _relationshipRepo.HasRelationshipAsync(user.Value.UserId, patientId);
                    if (!isPlanOwner && !hasActiveRelationship)
                        return Forbid();
                }
                else if (!await _relationshipRepo.HasRelationshipAsync(user.Value.UserId, patientId))
                {
                    return Forbid();
                }
            }
            else
            {
                return Forbid();
            }
            try
            {
                var planExercise = await _planExerciseRepo.GetByIdAsync(planExerciseId);
                if (planExercise == null)
                    return NotFound(new { error = "Plan exercise not found" });

                var plan = await _therapyPlanService.GetPlanWithExercisesForPatientAsync(planExercise.TherapyPlanId, patientId);
                if (plan == null)
                    return NotFound(new { error = "Plan exercise does not belong to this patient" });

                // Try fetching explicit vocabulary links for this exercise first
                var linkedVocab = (await _vocabExerciseRepo.GetByExerciseIdAsync(planExercise.ExerciseId)).ToList();
                List<Nutq.Core.Entities.Vocabulary> vocabularies;

                if (linkedVocab.Any())
                {
                    vocabularies = linkedVocab.Select(l => l.Vocabulary).ToList();
                }
                else
                {
                    var category = planExercise.Exercise?.Category ?? "tools";
                    var difficulty = planExercise.Exercise?.Difficulty ?? planExercise.Exercise?.DifficultyLevel?.Name ?? "Easy";

                    vocabularies = (await _vocabularyRepo.GetByCategoryAndDifficultyLevelAsync(category, difficulty)).ToList();
                    if (!vocabularies.Any())
                    {
                        vocabularies = (await _vocabularyRepo.GetByCategoryAndDifficultyLevelAsync(category, "Easy")).ToList();
                    }
                }

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
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null || user.Value.Role != "patient" || user.Value.UserId != patientId)
                return Forbid();
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
        public class CompleteExerciseRequest
        {
            public double? Score { get; set; }
            public string? SessionData { get; set; }
        }

        public class CompleteRepetitionRequest
        {
            public string? SessionData { get; set; }
        }

        [HttpPost("{patientId}/exercises/{planExerciseId}/complete")]
        public async Task<IActionResult> CompleteExercise(int patientId, int planExerciseId, [FromBody] CompleteExerciseRequest? body = null)
        {
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null || user.Value.Role != "patient" || user.Value.UserId != patientId)
                return Forbid();
            try
            {
                await _exerciseProgressService.CompleteExerciseAsync(patientId, planExerciseId, body?.Score, body?.SessionData);
                return Ok(new { message = "Exercise completed successfully", endTime = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Complete a single repetition - increments CurrentRepetition or marks as complete if all repetitions done.
        /// </summary>
        [HttpPost("{patientId}/exercises/{planExerciseId}/complete-repetition")]
        public async Task<IActionResult> CompleteRepetition(int patientId, int planExerciseId, [FromBody] CompleteRepetitionRequest? body = null)
        {
            var user = JwtAuthorizationHelper.GetCurrentUser(Request);
            if (user == null || user.Value.Role != "patient" || user.Value.UserId != patientId)
                return Forbid();
            try
            {
                await _exerciseProgressService.CompleteRepetitionAsync(patientId, planExerciseId, body?.SessionData);
                return Ok(new { message = "Repetition completed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get persisted session analytics (TrainingSession + SpeechAttempts) for a completed exercise.
        /// </summary>
        [HttpGet("{patientId}/exercises/{planExerciseId}/session-analytics")]
        public async Task<IActionResult> GetSessionAnalytics(int patientId, int planExerciseId)
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

                // Patient transfer rules: New doctor CANNOT see detailed session analytics/speech attempts from previous doctor's plans
                var planEx = await _planExerciseRepo.GetByIdAsync(planExerciseId);
                if (planEx == null)
                    return NotFound(new { error = "Plan exercise not found." });

                var plan = await _therapyPlanService.GetPlanByIdAsync(planEx.TherapyPlanId);
                if (plan == null || plan.DoctorId != user.Value.UserId)
                    return Forbid();
            }
            else
            {
                return Forbid();
            }
            try
            {
                var analytics = await _analyticsService.GetExerciseSessionAnalyticsAsync(patientId, planExerciseId);
                if (analytics == null)
                    return NotFound(new { error = "Session analytics not found. Complete the exercise first." });

                return Ok(MapSessionAnalytics(analytics));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private static PatientExerciseSessionAnalyticsDto MapSessionAnalytics(PatientExerciseSessionAnalytics a) =>
            new()
            {
                TrainingSessionId = a.TrainingSessionId,
                ExerciseProgressId = a.ExerciseProgressId,
                ExerciseName = a.ExerciseName,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                TotalDurationSeconds = a.TotalDurationSeconds,
                WordsCompleted = a.WordsCompleted,
                FirstAttemptCorrectCount = a.FirstAttemptCorrectCount,
                AccuracyPercent = a.AccuracyPercent,
                FirstAttemptSuccessRate = a.FirstAttemptSuccessRate,
                AverageSimilarityScore = a.AverageSimilarityScore,
                StrengthAreas = a.StrengthAreas,
                WeaknessAreas = a.WeaknessAreas,
                Words = a.Words.Select(w => new WordSessionPerformanceDto
                {
                    VocabularyId = w.VocabularyId,
                    ExpectedWord = w.ExpectedWord,
                    WordEnglish = w.WordEnglish,
                    WordArabic = w.WordArabic,
                    TotalAttempts = w.TotalAttempts,
                    FirstAttemptCorrect = w.FirstAttemptCorrect,
                    BestSimilarityScore = w.BestSimilarityScore,
                    AverageSimilarityScore = w.AverageSimilarityScore,
                    Succeeded = w.Succeeded,
                    Attempts = w.Attempts.Select(at => new SpeechAttemptSummaryDto
                    {
                        AttemptNumber = at.AttemptNumber,
                        ExpectedWord = at.ExpectedWord,
                        RecognizedWord = at.RecognizedWord,
                        SimilarityScore = at.SimilarityScore,
                        IsCorrect = at.IsCorrect,
                        AudioDurationSeconds = at.AudioDurationSeconds,
                        AttemptedAt = at.AttemptedAt
                    })
                })
            };
    }
}
