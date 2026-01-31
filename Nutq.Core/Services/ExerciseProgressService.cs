// Core/Services/ExerciseProgressService.cs
using Nutq.Core.Commands;
using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nutq.Core.Services
{
    public class ExerciseProgressService : IExerciseProgressService
    {
        private readonly IExerciseProgressRepository _progressRepo;
        private readonly IPatientRepository _patientRepo;
        private readonly IPlanExerciseRepository _planExerciseRepo;
        private readonly ITherapyPlanRepository _therapyPlanRepo;

        public ExerciseProgressService(
            IExerciseProgressRepository progressRepo,
            IPatientRepository patientRepo,
            IPlanExerciseRepository planExerciseRepo,
            ITherapyPlanRepository therapyPlanRepo)
        {
            _progressRepo = progressRepo;
            _patientRepo = patientRepo;
            _planExerciseRepo = planExerciseRepo;
            _therapyPlanRepo = therapyPlanRepo;
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
                // Update
                existing.StartTime = command.StartTime;
                existing.EndTime = command.EndTime;
                existing.Score = command.Score;
                existing.Completed = command.Completed;
                await _progressRepo.UpdateAsync(existing);
            }
            else
            {
                // Add new
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
                Completed = false
            };
            await _progressRepo.AddAsync(progress);
        }

        public async Task CompleteExerciseAsync(int patientId, int planExerciseId)
        {
            var planExercise = await _planExerciseRepo.GetByIdAsync(planExerciseId);
            if (planExercise == null)
                throw new Exception("Plan exercise not found");

            var plan = await _therapyPlanRepo.GetByIdAsync(planExercise.TherapyPlanId);
            if (plan == null || plan.PatientId != patientId)
                throw new Exception("Plan exercise does not belong to this patient");

            var existing = await _progressRepo.GetByPatientAndPlanExerciseAsync(patientId, planExerciseId);
            if (existing == null)
                throw new Exception("No started exercise found. Start the exercise first.");

            existing.EndTime = DateTime.UtcNow;
            existing.Completed = true;
            existing.Score = null;
            await _progressRepo.UpdateAsync(existing);
        }
    }
}
