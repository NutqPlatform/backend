using Microsoft.EntityFrameworkCore;
using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Infrastructure.Data;

namespace Nutq.Infrastructure.Repositories
{
    public class SessionClinicalReportRepository : ISessionClinicalReportRepository
    {
        private readonly ApplicationDbContext _context;

        public SessionClinicalReportRepository(ApplicationDbContext context) => _context = context;

        public async Task<SessionClinicalReport> AddAsync(SessionClinicalReport report)
        {
            _context.SessionClinicalReports.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task<SessionClinicalReport?> GetByTrainingSessionIdAsync(int trainingSessionId)
        {
            return await _context.SessionClinicalReports.AsNoTracking()
                .FirstOrDefaultAsync(r => r.TrainingSessionId == trainingSessionId);
        }

        public async Task<SessionClinicalReport?> GetByIdAsync(int id)
        {
            return await _context.SessionClinicalReports.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<SessionClinicalReport>> GetByPatientAsync(int patientId, DateTime? from = null, DateTime? to = null)
        {
            var query = _context.SessionClinicalReports.AsNoTracking()
                .Where(r => r.PatientId == patientId);

            if (from.HasValue) query = query.Where(r => r.GeneratedAt >= from.Value);
            if (to.HasValue) query = query.Where(r => r.GeneratedAt <= to.Value);

            return await query.OrderByDescending(r => r.GeneratedAt).ToListAsync();
        }
    }
}
