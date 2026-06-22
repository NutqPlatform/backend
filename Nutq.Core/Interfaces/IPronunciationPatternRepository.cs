using Nutq.Core.Entities;

namespace Nutq.Core.Interfaces
{
    public interface IPronunciationPatternRepository
    {
        Task<PronunciationPattern?> FindAsync(int patientId, string expectedPattern, string recognizedPattern, string patternType);
        Task<PronunciationPattern> AddAsync(PronunciationPattern pattern);
        Task UpdateAsync(PronunciationPattern pattern);
        Task<IEnumerable<PronunciationPattern>> GetActiveByPatientAsync(int patientId);
        Task<IEnumerable<PronunciationPattern>> GetByPatientAsync(int patientId);
    }
}
