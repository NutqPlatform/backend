using Nutq.Core.Entities;
using Nutq.Core.Models;

namespace Nutq.Core.Interfaces
{
    public interface ICategoryPerformanceSnapshotRepository
    {
        Task AddRangeAsync(IEnumerable<CategoryPerformanceSnapshot> snapshots);
        Task<IEnumerable<CategoryPerformanceSnapshot>> GetLatestByPatientAsync(int patientId);
        Task<IEnumerable<CategoryPerformanceSnapshot>> GetByPatientAsync(int patientId, DateTime? from = null, DateTime? to = null);
        Task<CategoryPerformanceSnapshot?> GetPreviousForCategoryAsync(int patientId, string category, DateTime beforeDate);
        Task<IReadOnlyList<PatientCategoryScoreProjection>> GetCategoryScoreProjectionsAsync(int patientId);
    }
}
