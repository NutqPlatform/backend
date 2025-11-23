using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Infrastructure.Data;

namespace Nutq.Infrastructure.Repositories
{
    public class ExerciseProgressRepository : Repository<ExerciseProgress>, IExerciseProgressRepository
    {
        public ExerciseProgressRepository(ApplicationDbContext context) : base(context)
        {
        }

        
    }
}
