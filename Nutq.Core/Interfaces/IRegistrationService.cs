using Nutq.Core.Commands;

namespace Nutq.Core.Interfaces
{
    public interface IRegistrationService
    {
        Task<bool> RegisterDoctorAsync(DoctorRegisterCommand command);
        Task<bool> RegisterPatientAsync(PatientRegisterCommand command);
    }
}
