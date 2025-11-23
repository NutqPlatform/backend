using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Infrastructure.Data;

namespace Nutq.Infrastructure.Repositories
{
    public class PlanExerciseRepository : Repository<PlanExercise>, IPlanExerciseRepository
    {
        public PlanExerciseRepository(ApplicationDbContext context) : base(context)
        {
        }

        
    }
}
