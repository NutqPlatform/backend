using Nutq.Core.Entities;

namespace Nutq.Core.Interfaces
{
    public interface IDoctorReviewService
    {
        Task<DoctorReview> CreateReviewAsync(int doctorId, int patientId, int rating, string? comment);
        Task<DoctorReview?> UpdateReviewAsync(int doctorId, int patientId, int rating, string? comment);
        Task<IEnumerable<DoctorReview>> GetDoctorReviewsAsync(int doctorId);
        Task<double> GetDoctorAverageRatingAsync(int doctorId);
        Task<bool> DeleteReviewAsync(int doctorId, int patientId);
    }
}
