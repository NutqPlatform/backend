using Microsoft.EntityFrameworkCore;
using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Infrastructure.Data;

namespace Nutq.Infrastructure.Repositories
{
    public class SpeechAttemptRepository : ISpeechAttemptRepository
    {
        private readonly ApplicationDbContext _context;

        public SpeechAttemptRepository(ApplicationDbContext context) => _context = context;

        public async Task AddRangeAsync(IEnumerable<SpeechAttempt> attempts)
        {
            _context.SpeechAttempts.AddRange(attempts);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<SpeechAttempt>> GetByPatientAsync(int patientId, DateTime? from = null, DateTime? to = null)
        {
            var query = _context.SpeechAttempts.AsNoTracking().Where(a => a.PatientId == patientId);
            if (from.HasValue) query = query.Where(a => a.AttemptedAt >= from.Value);
            if (to.HasValue) query = query.Where(a => a.AttemptedAt <= to.Value);
            return await query.OrderBy(a => a.AttemptedAt).ToListAsync();
        }

        public async Task<IEnumerable<SpeechAttempt>> GetByTrainingSessionAsync(int trainingSessionId)
        {
            return await _context.SpeechAttempts.AsNoTracking()
                .Where(a => a.TrainingSessionId == trainingSessionId)
                .OrderBy(a => a.AttemptedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<SpeechAttempt>> GetByTrainingSessionIdsAsync(IEnumerable<int> trainingSessionIds)
        {
            var ids = trainingSessionIds.ToList();
            return await _context.SpeechAttempts.AsNoTracking()
                .Where(a => ids.Contains(a.TrainingSessionId))
                .OrderBy(a => a.AttemptedAt)
                .ToListAsync();
        }
    }
}
