using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Nutq.Infrastructure.Repositories
{
    public class WeeklyReportRepository : Repository<WeeklyReport>, IWeeklyReportRepository
    {
        public WeeklyReportRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<WeeklyReport?> GetByTherapyPlanIdAsync(int planId)
        {
            return await _context.WeeklyReports
                .Include(wr => wr.Doctor)
                .Include(wr => wr.Patient)
                .Include(wr => wr.TherapyPlan)
                .FirstOrDefaultAsync(wr => wr.TherapyPlanId == planId);
        }

        public async Task<List<WeeklyReport>> GetByPatientIdAsync(int patientId)
        {
            return await _context.WeeklyReports
                .Where(wr => wr.PatientId == patientId)
                .Include(wr => wr.Doctor)
                .Include(wr => wr.TherapyPlan)
                .OrderByDescending(wr => wr.EndDate)
                .ToListAsync();
        }
    }
}
