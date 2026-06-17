using Nutq.Core.Entities;

namespace Nutq.Core.Interfaces
{
    public interface IDoctorReviewRepository : IRepository<DoctorReview>
    {
        Task<IEnumerable<DoctorReview>> GetReviewsByDoctorIdAsync(int doctorId);
        Task<DoctorReview?> GetReviewAsync(int doctorId, int patientId);
        Task<double> GetAverageRatingAsync(int doctorId);
    }
}
