using System.Threading.Tasks;

namespace Nutq.Core.Interfaces
{
    public interface IPatientService
    {
        Task<object> GetPatientProfileAsync(int patientId);
        Task UpdatePatientProfileAsync(int patientId, string? profilePicture, string? phoneNumber = null, DateTime? dateOfBirth = null);
        Task UpdatePatientPasswordAsync(int patientId, string currentPassword, string newPassword);
        Task<object?> GetAttendingDoctorAsync(int patientId);
    }
}
