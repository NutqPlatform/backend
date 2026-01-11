using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
    }
}
