// Core/Services/ExerciseProgressService.cs
using Nutq.Core.Commands;
using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nutq.Core.Services
{
    public class ExerciseProgressService : IExerciseProgressService
    {
        private readonly IExerciseProgressRepository _progressRepo;
        private readonly IPatientRepository _patientRepo;
        private readonly IPlanExerciseRepository _planExerciseRepo;
        private readonly ITherapyPlanRepository _therapyPlanRepo;
        private readonly IPatientAnalyticsIngestionService _analyticsIngestion;

        public ExerciseProgressService(
            IExerciseProgressRepository progressRepo,
            IPatientRepository patientRepo,
            IPlanExerciseRepository planExerciseRepo,
            ITherapyPlanRepository therapyPlanRepo,
            IPatientAnalyticsIngestionService analyticsIngestion)
        {
            _progressRepo = progressRepo;
            _patientRepo = patientRepo;
            _planExerciseRepo = planExerciseRepo;
            _therapyPlanRepo = therapyPlanRepo;
            _analyticsIngestion = analyticsIngestion;
        }

        public async Task AddOrUpdateProgressAsync(ExerciseProgressCommand command)
        {
            var patient = await _patientRepo.GetByIdAsync(command.PatientId);
            if (patient == null)
                throw new Exception("Patient not found");

            var planExercise = await _planExerciseRepo.GetByIdAsync(command.PlanExerciseId);
            if (planExercise == null)
                throw new Exception("Plan exercise not found");

            // Check existing record
            var existing = await _progressRepo.GetByPatientAndPlanExerciseAsync(command.PatientId, command.PlanExerciseId);
            if (existing != null)
            {
                existing.StartTime = command.StartTime;
                existing.EndTime = command.EndTime;
                existing.Score = command.Score;
                existing.Completed = command.Completed;
                await _progressRepo.UpdateAsync(existing);

                if (command.Completed)
                {
                    var plan = await _therapyPlanRepo.GetByIdAsync(planExercise.TherapyPlanId);
                    if (plan != null)
                        await CheckAndCompletePlanIfAllExercisesDoneAsync(plan, command.PatientId);
                }
            }
            else
            {
                var progress = new ExerciseProgress
                {
                    PatientId = command.PatientId,
                    PlanExerciseId = command.PlanExerciseId,
                    StartTime = command.StartTime,
                    EndTime = command.EndTime,
                    Score = command.Score,
                    Completed = command.Completed
                };
                await _progressRepo.AddAsync(progress);

                if (command.Completed)
                {
                    var plan = await _therapyPlanRepo.GetByIdAsync(planExercise.TherapyPlanId);
                    if (plan != null)
                        await CheckAndCompletePlanIfAllExercisesDoneAsync(plan, command.PatientId);
                }
            }
        }

        public async Task<IEnumerable<ExerciseProgress>> GetPatientProgressAsync(int patientId)
        {
            return await _progressRepo.GetByPatientAsync(patientId);
        }

        public async Task StartExerciseAsync(int patientId, int planExerciseId)
        {
            var patient = await _patientRepo.GetByIdAsync(patientId);
            if (patient == null)
                throw new Exception("Patient not found");

            var planExercise = await _planExerciseRepo.GetByIdAsync(planExerciseId);
            if (planExercise == null)
                throw new Exception("Plan exercise not found");

            var plan = await _therapyPlanRepo.GetByIdAsync(planExercise.TherapyPlanId);
            if (plan == null || plan.PatientId != patientId)
                throw new Exception("Plan exercise does not belong to this patient");
            if (plan.Status != "Active")
                throw new Exception("Can only exercise on the active therapy plan");

            var existing = await _progressRepo.GetByPatientAndPlanExerciseAsync(patientId, planExerciseId);
            if (existing != null)
                throw new Exception("Exercise already started. Use complete to finish.");

            var progress = new ExerciseProgress
            {
                PatientId = patientId,
                PlanExerciseId = planExerciseId,
                StartTime = DateTime.UtcNow,
                EndTime = null,
                Score = null,
                Completed = false,
                CurrentRepetition = 1,
                TotalRepetitions = planExercise.Repetition
            };
            await _progressRepo.AddAsync(progress);
        }

        public async Task CompleteExerciseAsync(int patientId, int planExerciseId, double? score = null, string? sessionData = null)
        {
            var planExercise = await _planExerciseRepo.GetByIdAsync(planExerciseId);
            if (planExercise == null)
                throw new Exception("Plan exercise not found");

            var plan = await _therapyPlanRepo.GetByIdAsync(planExercise.TherapyPlanId);
            if (plan == null || plan.PatientId != patientId)
                throw new Exception("Plan exercise does not belong to this patient");
            if (plan.Status != "Active")
                throw new Exception("Can only exercise on the active therapy plan");

            var existing = await _progressRepo.GetByPatientAndPlanExerciseAsync(patientId, planExerciseId);
            if (existing == null)
                throw new Exception("No started exercise found. Start the exercise first.");

            existing.EndTime = DateTime.UtcNow;
            existing.Completed = true;
            existing.Score = score;
            if (sessionData != null)
                existing.SessionData = sessionData;
            await _progressRepo.UpdateAsync(existing);
            await TryIngestAnalyticsAsync(existing);

            await CheckAndCompletePlanIfAllExercisesDoneAsync(plan, patientId);
        }

        public async Task CompleteRepetitionAsync(int patientId, int planExerciseId, string? sessionData = null)
        {
            var existing = await _progressRepo.GetByPatientAndPlanExerciseAsync(patientId, planExerciseId);
            if (existing == null)
                throw new Exception("No started exercise found. Start the exercise first.");

            // Accumulate session data
            if (sessionData != null)
                existing.SessionData = sessionData;

            // Move to next repetition
            if (existing.CurrentRepetition < existing.TotalRepetitions)
            {
                existing.CurrentRepetition++;
                await _progressRepo.UpdateAsync(existing);
            }
            else if (existing.CurrentRepetition == existing.TotalRepetitions)
            {
                // All repetitions completed, mark as complete
                existing.EndTime = DateTime.UtcNow;
                existing.Completed = true;
                await _progressRepo.UpdateAsync(existing);

                var planExercise = await _planExerciseRepo.GetByIdAsync(planExerciseId);
                if (planExercise != null)
                {
                    var plan = await _therapyPlanRepo.GetByIdAsync(planExercise.TherapyPlanId);
                    if (plan != null)
                        await CheckAndCompletePlanIfAllExercisesDoneAsync(plan, patientId);
                }

                await TryIngestAnalyticsAsync(existing);
            }
        }

        private async Task TryIngestAnalyticsAsync(ExerciseProgress progress)
        {
            try
            {
                await _analyticsIngestion.IngestCompletedSessionAsync(progress);
            }
            catch
            {
                // Analytics ingestion must not block exercise completion
            }
        }

        /// <summary>
        /// If patient did all exercises in plan, plan is complete.
        /// </summary>
        private async Task CheckAndCompletePlanIfAllExercisesDoneAsync(TherapyPlan plan, int patientId)
        {
            if (plan.Status == "Completed")
                return;

            var planExercises = await _planExerciseRepo.GetByPlanIdsAsync(new List<int> { plan.Id });
            var planExerciseIds = planExercises.Select(pe => pe.Id).ToList();
            if (planExerciseIds.Count == 0)
                return;

            var allDone = true;
            foreach (var peId in planExerciseIds)
            {
                var progress = await _progressRepo.GetByPatientAndPlanExerciseAsync(patientId, peId);
                if (progress == null || !progress.Completed)
                {
                    allDone = false;
                    break;
                }
            }

            if (allDone)
            {
                plan.Status = "Completed";
                await _therapyPlanRepo.UpdateAsync(plan);
            }
        }
    }
}