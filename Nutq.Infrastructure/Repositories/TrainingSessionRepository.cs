using Microsoft.EntityFrameworkCore;
using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Core.Models;
using Nutq.Infrastructure.Data;

namespace Nutq.Infrastructure.Repositories
{
    public class TrainingSessionRepository : ITrainingSessionRepository
    {
        private readonly ApplicationDbContext _context;

        public TrainingSessionRepository(ApplicationDbContext context) => _context = context;

        public async Task<TrainingSession> AddAsync(TrainingSession session)
        {
            _context.TrainingSessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<TrainingSession?> GetByExerciseProgressIdAsync(int exerciseProgressId)
        {
            return await _context.TrainingSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ExerciseProgressId == exerciseProgressId);
        }

        public async Task<TrainingSession?> GetByPatientAndPlanExerciseAsync(int patientId, int planExerciseId)
        {
            return await _context.TrainingSessions
                .AsNoTracking()
                .Include(s => s.Exercise)
                .Include(s => s.ClinicalReport)
                .FirstOrDefaultAsync(s => s.PatientId == patientId && s.PlanExerciseId == planExerciseId);
        }

        public async Task<TrainingSession?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.TrainingSessions
                .AsNoTracking()
                .Include(s => s.Exercise)
                .Include(s => s.ClinicalReport)
                .Include(s => s.ProgressSnapshot)
                    .ThenInclude(p => p!.CategoryPerformances)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<TrainingSession>> GetByPatientAsync(int patientId, DateTime? from = null, DateTime? to = null)
        {
            var query = _context.TrainingSessions.AsNoTracking()
                .Include(s => s.Exercise)
                .Where(s => s.PatientId == patientId);

            if (from.HasValue) query = query.Where(s => s.StartTime >= from.Value);
            if (to.HasValue) query = query.Where(s => s.EndTime <= to.Value);

            return await query.OrderByDescending(s => s.StartTime).ToListAsync();
        }

        public async Task<int> GetTotalDurationSecondsAsync(int patientId)
        {
            return await _context.TrainingSessions.AsNoTracking()
                .Where(s => s.PatientId == patientId)
                .SumAsync(s => s.TotalDurationSeconds);
        }

        public async Task<IReadOnlyList<PatientSessionTimelineProjection>> GetSessionTimelineProjectionsAsync(int patientId)
        {
            return await _context.TrainingSessions.AsNoTracking()
                .Where(ts => ts.PatientId == patientId)
                .OrderBy(ts => ts.StartTime)
                .Select(ts => new PatientSessionTimelineProjection(
                    ts.Id,
                    ts.StartTime,
                    ts.EndTime,
                    ts.ProgressSnapshot != null ? ts.ProgressSnapshot.AccuracyPercent : ts.AccuracyPercent,
                    ts.ProgressSnapshot != null ? ts.ProgressSnapshot.Id : (int?)null))
                .ToListAsync();
        }

        public async Task<IEnumerable<TrainingSession>> GetByPlanExerciseIdsAsync(IEnumerable<int> planExerciseIds)
        {
            var ids = planExerciseIds.ToList();
            return await _context.TrainingSessions.AsNoTracking()
                .Include(s => s.Exercise)
                .Where(s => ids.Contains(s.PlanExerciseId))
                .OrderBy(s => s.StartTime)
                .ToListAsync();
        }
    }
}
