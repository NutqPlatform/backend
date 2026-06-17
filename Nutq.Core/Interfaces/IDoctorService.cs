using System.Threading.Tasks;
using Nutq.Core.Entities;

namespace Nutq.Core.Interfaces
{
    public interface IDoctorService
{
    Task<string> GeneratePatientCodeAsync(int doctorId);
    
    Task<IEnumerable<object>> GetDoctorPatientsAsync(int doctorId); // بدل Patient
    
    Task<object?> GetPatientByIdAsync(int doctorId, int patientId);
    
    Task UpdatePatientDiagnosisAsync(int doctorId, int patientId, string diagnosis, string? diagnosisFileUrl = null);
    
    Task<object> GetDoctorProfileAsync(int doctorId);
    
    Task UpdateDoctorProfileAsync(int doctorId, string? profilePicture, string? cv,
        string? name = null, string? phoneNumber = null, string? communicationInfo = null, string? address = null, DateTime? dateOfBirth = null, string? cvText = null);
    
    Task UpdateDoctorPasswordAsync(int doctorId, string currentPassword, string newPassword);

    Task<IEnumerable<object>> GetAllDoctorsWithCommunicationsAsync();

    Task<object> GetDoctorWithCommunicationsAsync(int doctorId);
}

}
