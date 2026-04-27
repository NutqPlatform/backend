using Nutq.Core.Auth;

namespace Nutq.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult?> LoginDoctorAsync(string email, string password);
        Task<AuthResult?> LoginPatientAsync(string email, string password);
    }
}
