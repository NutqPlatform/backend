using Nutq.Core.Auth;

namespace Nutq.Core.Interfaces
{
    public interface IAdminService
    {
        Task<AuthResult> LoginAsync(string email, string password);
        Task<bool> BlockDoctorAsync(int doctorId);
        Task<bool> UnblockDoctorAsync(int doctorId);
        Task<bool> BlockPatientAsync(int patientId);
        Task<bool> UnblockPatientAsync(int patientId);
    }
}
