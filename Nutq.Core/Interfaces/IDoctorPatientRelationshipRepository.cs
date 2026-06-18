using Nutq.Core.Entities;

namespace Nutq.Core.Interfaces
{
    public interface IDoctorPatientRelationshipRepository : IRepository<DoctorPatientRelationship>
    {
        Task<DoctorPatientRelationship?> GetActiveAsync(int doctorId, int patientId);
        Task<DoctorPatientRelationship?> GetActiveForPatientAsync(int patientId);
        Task<IEnumerable<DoctorPatientRelationship>> GetActiveByDoctorIdAsync(int doctorId);
        Task<IEnumerable<DoctorPatientRelationship>> GetFormerByDoctorIdAsync(int doctorId);
        Task<IEnumerable<DoctorPatientRelationship>> GetByDoctorIdAsync(int doctorId);
        Task<IEnumerable<DoctorPatientRelationship>> GetByPatientIdAsync(int patientId);
        Task<DoctorPatientRelationship?> GetLastEndedForPatientAsync(int patientId);
        Task<bool> HasRelationshipAsync(int doctorId, int patientId);
        Task<int> CountDistinctPatientsAsync(int doctorId);
    }
}
