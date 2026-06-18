using Nutq.Core.Entities;

namespace Nutq.Core.Interfaces
{
    public interface ITransferRequestRepository : IRepository<TransferRequest>
    {
        Task<TransferRequest?> GetPendingForPatientAndDoctorAsync(int patientId, int toDoctorId);
        Task<IEnumerable<TransferRequest>> GetPendingByDoctorIdAsync(int doctorId);
        Task<IEnumerable<TransferRequest>> GetByPatientIdAsync(int patientId);
        Task<TransferRequest?> GetByIdWithDetailsAsync(int id);
    }
}
