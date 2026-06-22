using Nutq.Core.Entities;

namespace Nutq.Core.Interfaces
{
    public interface ISpeechAttemptRepository
    {
        Task AddRangeAsync(IEnumerable<SpeechAttempt> attempts);
        Task<IEnumerable<SpeechAttempt>> GetByPatientAsync(int patientId, DateTime? from = null, DateTime? to = null);
        Task<IEnumerable<SpeechAttempt>> GetByTrainingSessionAsync(int trainingSessionId);
    }
}
