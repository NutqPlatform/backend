using Nutq.Core.Entities;

namespace Nutq.Core.Interfaces
{
    public interface IProgressSnapshotRepository
    {
        Task<ProgressSnapshot> AddAsync(ProgressSnapshot snapshot);
        Task<ProgressSnapshot?> GetLatestAsync(int patientId);
        Task<ProgressSnapshot?> GetPreviousAsync(int patientId, DateTime beforeDate);
        Task<IEnumerable<ProgressSnapshot>> GetByPatientAsync(int patientId, DateTime? from = null, DateTime? to = null);
    }
}
