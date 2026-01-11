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

        public ExerciseProgressService(
            IExerciseProgressRepository progressRepo,
            IPatientRepository patientRepo,
            IPlanExerciseRepository planExerciseRepo)
        {
            _progressRepo = progressRepo;
            _patientRepo = patientRepo;
            _planExerciseRepo = planExerciseRepo;
        }

        public async Task AddProgressAsync(ExerciseProgressCommand command)
        {
            var patient = await _patientRepo.GetByIdAsync(command.PatientId);
            if (patient == null)
                throw new Exception("Patient not found");

            var planExercise = await _planExerciseRepo.GetByIdAsync(command.PlanExerciseId);
            if (planExercise == null)
                throw new Exception("Plan exercise not found");

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

        public async Task<IEnumerable<ExerciseProgress>> GetPatientProgressAsync(int patientId)
        {
            return await _progressRepo.GetByPatientAsync(patientId);
        }
    }
}
