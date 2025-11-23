using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Infrastructure.Data;

namespace Nutq.Infrastructure.Repositories
{
    public class WeeklyReportRepository : Repository<WeeklyReport>, IWeeklyReportRepository
    {
        public WeeklyReportRepository(ApplicationDbContext context) : base(context)
        {
        }

        
    }
}
