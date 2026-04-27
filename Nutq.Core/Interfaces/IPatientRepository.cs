using Nutq.Core.Entities;

namespace Nutq.Core.Interfaces
{
    public interface IPatientRepository : IRepository<Patient>
{
    Task<Patient?> GetByEmailAsync(string email);
    Task<IEnumerable<Patient>> GetByDoctorIdAsync(int doctorId);
    Task<List<TherapyPlan>> GetPatientPlansAsync(int patientId);
    Task<List<ExerciseProgress>> GetPatientProgressAsync(int patientId);

}

}
