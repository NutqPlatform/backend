// Infrastructure/Repositories/ExerciseProgressRepository.cs
using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nutq.Infrastructure.Repositories
{
    public class ExerciseProgressRepository : IExerciseProgressRepository
    {
        private readonly ApplicationDbContext _context;

        public ExerciseProgressRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ExerciseProgress progress)
        {
            _context.ExerciseProgresses.Add(progress);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ExerciseProgress progress)
        {
            _context.ExerciseProgresses.Update(progress);
            await _context.SaveChangesAsync();
        }

        public async Task<ExerciseProgress?> GetByPatientAndPlanExerciseAsync(int patientId, int planExerciseId)
        {
            return await _context.ExerciseProgresses
                .Include(ep => ep.PlanExercise)
                    .ThenInclude(pe => pe.Exercise)
                .FirstOrDefaultAsync(ep => ep.PatientId == patientId && ep.PlanExerciseId == planExerciseId);
        }

        public async Task<IEnumerable<ExerciseProgress>> GetByPatientAsync(int patientId)
        {
            return await _context.ExerciseProgresses
                .Where(ep => ep.PatientId == patientId)
                .Include(ep => ep.PlanExercise)
                    .ThenInclude(pe => pe.Exercise)
                .ToListAsync();
        }

        public async Task<IEnumerable<ExerciseProgress>> GetByPlanExerciseAsync(int planExerciseId)
        {
            return await _context.ExerciseProgresses
                .Include(ep => ep.Patient)
                .Where(ep => ep.PlanExerciseId == planExerciseId)
                .ToListAsync();
        }

        public async Task<List<ExerciseProgress>> GetByPlanIdsAsync(List<int> planIds)
        {
            return await _context.ExerciseProgresses
                .Include(p => p.PlanExercise)
                .Where(p => planIds.Contains(p.PlanExercise.TherapyPlanId))
                .ToListAsync();
        }

        public async Task<List<ExerciseProgress>> GetByPlanExerciseIdsAsync(List<int> planExerciseIds)
        {
            return await _context.ExerciseProgresses
                .Include(ep => ep.Patient)
                .Include(ep => ep.PlanExercise)
                    .ThenInclude(pe => pe.Exercise)
                .Where(ep => planExerciseIds.Contains(ep.PlanExerciseId))
                .ToListAsync();
        }
    }
}
