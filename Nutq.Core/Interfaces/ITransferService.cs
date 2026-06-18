using Nutq.Core.Entities;

namespace Nutq.Core.Interfaces
{
    public interface ITransferService
    {
        Task LeaveDoctorAsync(int patientId);
        Task ReleasePatientAsync(int doctorId, int patientId);
        Task<TransferRequest> RequestTransferAsync(int patientId, int toDoctorId, string? message);
        Task<TransferRequest> DoctorInitiateTransferAsync(int doctorId, int patientId, int toDoctorId, string? message);
        Task<TransferRequest> AcceptTransferAsync(int doctorId, int requestId);
        Task<TransferRequest> RejectTransferAsync(int doctorId, int requestId);
        Task CancelTransferRequestAsync(int patientId, int requestId);
        Task<IEnumerable<object>> GetPendingRequestsForDoctorAsync(int doctorId);
        Task<IEnumerable<object>> GetRequestsForPatientAsync(int patientId);
        Task<IEnumerable<object>> GetFormerPatientsAsync(int doctorId);
    }
}
