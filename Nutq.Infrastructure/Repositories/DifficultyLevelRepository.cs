using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Infrastructure.Data;

namespace Nutq.Infrastructure.Repositories
{
    public class DifficultyLevelRepository : Repository<DifficultyLevel>, IDifficultyLevelRepository
    {
        public DifficultyLevelRepository(ApplicationDbContext context) : base(context)
        {
        }

        
    }
}
