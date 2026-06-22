using Microsoft.EntityFrameworkCore;
using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Core.Models;
using Nutq.Infrastructure.Data;

namespace Nutq.Infrastructure.Repositories
{
    public class CategoryPerformanceSnapshotRepository : ICategoryPerformanceSnapshotRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryPerformanceSnapshotRepository(ApplicationDbContext context) => _context = context;

        public async Task AddRangeAsync(IEnumerable<CategoryPerformanceSnapshot> snapshots)
        {
            _context.CategoryPerformanceSnapshots.AddRange(snapshots);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CategoryPerformanceSnapshot>> GetLatestByPatientAsync(int patientId)
        {
            var latestSnapshotId = await _context.ProgressSnapshots.AsNoTracking()
                .Where(p => p.PatientId == patientId)
                .OrderByDescending(p => p.SnapshotDate)
                .Select(p => p.Id)
                .FirstOrDefaultAsync();

            if (latestSnapshotId == 0) return Array.Empty<CategoryPerformanceSnapshot>();

            return await _context.CategoryPerformanceSnapshots.AsNoTracking()
                .Where(c => c.ProgressSnapshotId == latestSnapshotId)
                .OrderByDescending(c => c.AccuracyPercent)
                .ToListAsync();
        }

        public async Task<IEnumerable<CategoryPerformanceSnapshot>> GetByPatientAsync(int patientId, DateTime? from = null, DateTime? to = null)
        {
            var query = _context.CategoryPerformanceSnapshots.AsNoTracking()
                .Include(c => c.ProgressSnapshot)
                .Where(c => c.PatientId == patientId);

            if (from.HasValue) query = query.Where(c => c.ProgressSnapshot!.SnapshotDate >= from.Value);
            if (to.HasValue) query = query.Where(c => c.ProgressSnapshot!.SnapshotDate <= to.Value);

            return await query.OrderBy(c => c.ProgressSnapshot!.SnapshotDate).ToListAsync();
        }

        public async Task<CategoryPerformanceSnapshot?> GetPreviousForCategoryAsync(int patientId, string category, DateTime beforeDate)
        {
            return await _context.CategoryPerformanceSnapshots.AsNoTracking()
                .Include(c => c.ProgressSnapshot)
                .Where(c => c.PatientId == patientId
                    && c.Category == category
                    && c.ProgressSnapshot!.SnapshotDate < beforeDate)
                .OrderByDescending(c => c.ProgressSnapshot!.SnapshotDate)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<PatientCategoryScoreProjection>> GetCategoryScoreProjectionsAsync(int patientId)
        {
            return await _context.CategoryPerformanceSnapshots.AsNoTracking()
                .Where(c => c.PatientId == patientId)
                .Join(
                    _context.ProgressSnapshots.AsNoTracking(),
                    category => category.ProgressSnapshotId,
                    snapshot => snapshot.Id,
                    (category, snapshot) => new { category, snapshot })
                .OrderBy(x => x.snapshot.SnapshotDate)
                .Select(x => new PatientCategoryScoreProjection(
                    x.category.ProgressSnapshotId,
                    x.snapshot.TrainingSessionId,
                    x.category.Category,
                    x.category.AccuracyPercent))
                .ToListAsync();
        }
    }
}
