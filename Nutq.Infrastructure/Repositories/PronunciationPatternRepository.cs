using Microsoft.EntityFrameworkCore;
using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Infrastructure.Data;

namespace Nutq.Infrastructure.Repositories
{
    public class PronunciationPatternRepository : IPronunciationPatternRepository
    {
        private readonly ApplicationDbContext _context;

        public PronunciationPatternRepository(ApplicationDbContext context) => _context = context;

        public async Task<PronunciationPattern?> FindAsync(int patientId, string expectedPattern, string recognizedPattern, string patternType)
        {
            return await _context.PronunciationPatterns
                .FirstOrDefaultAsync(p =>
                    p.PatientId == patientId
                    && p.ExpectedPattern == expectedPattern
                    && p.RecognizedPattern == recognizedPattern
                    && p.PatternType == patternType);
        }

        public async Task<PronunciationPattern> AddAsync(PronunciationPattern pattern)
        {
            _context.PronunciationPatterns.Add(pattern);
            await _context.SaveChangesAsync();
            return pattern;
        }

        public async Task UpdateAsync(PronunciationPattern pattern)
        {
            pattern.UpdatedAt = DateTime.UtcNow;
            _context.PronunciationPatterns.Update(pattern);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<PronunciationPattern>> GetActiveByPatientAsync(int patientId)
        {
            return await _context.PronunciationPatterns.AsNoTracking()
                .Where(p => p.PatientId == patientId && p.IsActive)
                .OrderByDescending(p => p.SeverityScore)
                .ThenByDescending(p => p.OccurrenceCount)
                .ToListAsync();
        }

        public async Task<IEnumerable<PronunciationPattern>> GetByPatientAsync(int patientId)
        {
            return await _context.PronunciationPatterns.AsNoTracking()
                .Where(p => p.PatientId == patientId)
                .OrderByDescending(p => p.LastDetectedAt)
                .ToListAsync();
        }
    }
}
