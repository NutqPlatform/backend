using System.Threading.Tasks;
using Nutq.Core.Entities;

namespace Nutq.Core.Interfaces
{
    public interface IDoctorService
{
    Task<string> GeneratePatientCodeAsync(int doctorId);
    
    Task<IEnumerable<object>> GetDoctorPatientsAsync(int doctorId); // بدل Patient
    
    Task<object?> GetPatientByIdAsync(int doctorId, int patientId);
    
    Task UpdatePatientDiagnosisAsync(int doctorId, int patientId, string diagnosis);
    
    Task<object> GetDoctorProfileAsync(int doctorId);
    
    Task UpdateDoctorProfileAsync(int doctorId, string? profilePicture, string? cv);
    
    Task UpdateDoctorPasswordAsync(int doctorId, string currentPassword, string newPassword);
}

}
