using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace Nutq.Infrastructure.Repositories
{
     public class PlanExerciseRepository : Repository<PlanExercise>, IPlanExerciseRepository
    {


        public PlanExerciseRepository(ApplicationDbContext context) : base(context)
        {
            
        }

        public async Task<List<PlanExercise>> GetByPlanIdsAsync(List<int> planIds)
        {
            return await _context.PlanExercises
                .Where(pe => planIds.Contains(pe.TherapyPlanId))
                .ToListAsync();
        }
    }
}
