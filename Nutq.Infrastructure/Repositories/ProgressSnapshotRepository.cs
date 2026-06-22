using Microsoft.EntityFrameworkCore;
using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Infrastructure.Data;

namespace Nutq.Infrastructure.Repositories
{
    public class ProgressSnapshotRepository : IProgressSnapshotRepository
    {
        private readonly ApplicationDbContext _context;

        public ProgressSnapshotRepository(ApplicationDbContext context) => _context = context;

        public async Task<ProgressSnapshot> AddAsync(ProgressSnapshot snapshot)
        {
            _context.ProgressSnapshots.Add(snapshot);
            await _context.SaveChangesAsync();
            return snapshot;
        }

        public async Task<ProgressSnapshot?> GetLatestAsync(int patientId)
        {
            return await _context.ProgressSnapshots.AsNoTracking()
                .Include(p => p.CategoryPerformances)
                .Where(p => p.PatientId == patientId)
                .OrderByDescending(p => p.SnapshotDate)
                .FirstOrDefaultAsync();
        }

        public async Task<ProgressSnapshot?> GetPreviousAsync(int patientId, DateTime beforeDate)
        {
            return await _context.ProgressSnapshots.AsNoTracking()
                .Where(p => p.PatientId == patientId && p.SnapshotDate < beforeDate)
                .OrderByDescending(p => p.SnapshotDate)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ProgressSnapshot>> GetByPatientAsync(int patientId, DateTime? from = null, DateTime? to = null)
        {
            var query = _context.ProgressSnapshots.AsNoTracking()
                .Include(p => p.CategoryPerformances)
                .Where(p => p.PatientId == patientId);

            if (from.HasValue) query = query.Where(p => p.SnapshotDate >= from.Value);
            if (to.HasValue) query = query.Where(p => p.SnapshotDate <= to.Value);

            return await query.OrderBy(p => p.SnapshotDate).ToListAsync();
        }
    }
}
