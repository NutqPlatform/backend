using Nutq.Core.Entities;

namespace Nutq.Core.Interfaces
{
    public interface ISessionClinicalReportRepository
    {
        Task<SessionClinicalReport> AddAsync(SessionClinicalReport report);
        Task<SessionClinicalReport?> GetByTrainingSessionIdAsync(int trainingSessionId);
        Task<SessionClinicalReport?> GetByIdAsync(int id);
        Task<IEnumerable<SessionClinicalReport>> GetByPatientAsync(int patientId, DateTime? from = null, DateTime? to = null);
        Task<IEnumerable<SessionClinicalReport>> GetByTrainingSessionIdsAsync(IEnumerable<int> trainingSessionIds);
    }
}
