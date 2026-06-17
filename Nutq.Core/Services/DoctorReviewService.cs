using Nutq.Core.Entities;
using Nutq.Core.Interfaces;

namespace Nutq.Core.Services
{
    public class DoctorReviewService : IDoctorReviewService
    {
        private readonly IDoctorReviewRepository _reviewRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;

        public DoctorReviewService(
            IDoctorReviewRepository reviewRepository,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository)
        {
            _reviewRepository = reviewRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
        }

        private async Task EnsurePatientCanReviewDoctorAsync(int doctorId, int patientId)
        {
            var patient = await _patientRepository.GetByIdAsync(patientId);
            if (patient == null)
                throw new ArgumentException("Patient not found");

            if (patient.DoctorId != doctorId)
                throw new ArgumentException("You can only review your assigned doctor");
        }

        public async Task<DoctorReview> CreateReviewAsync(int doctorId, int patientId, int rating, string? comment)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5");

            await EnsurePatientCanReviewDoctorAsync(doctorId, patientId);

            // Check if review already exists
            var existingReview = await _reviewRepository.GetReviewAsync(doctorId, patientId);
            if (existingReview != null)
                throw new InvalidOperationException("Patient has already reviewed this doctor");

            var review = new DoctorReview
            {
                DoctorId = doctorId,
                PatientId = patientId,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepository.AddAsync(review);

            // Update doctor's average rating
            await UpdateDoctorAverageRatingAsync(doctorId);

            return review;
        }

        public async Task<DoctorReview?> UpdateReviewAsync(int doctorId, int patientId, int rating, string? comment)
        {
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5");

            await EnsurePatientCanReviewDoctorAsync(doctorId, patientId);

            var review = await _reviewRepository.GetReviewAsync(doctorId, patientId);
            if (review == null)
                return null;

            review.Rating = rating;
            review.Comment = comment;
            review.CreatedAt = DateTime.UtcNow;

            await _reviewRepository.UpdateAsync(review);

            // Update doctor's average rating
            await UpdateDoctorAverageRatingAsync(doctorId);

            return review;
        }

        public async Task<IEnumerable<DoctorReview>> GetDoctorReviewsAsync(int doctorId)
        {
            return await _reviewRepository.GetReviewsByDoctorIdAsync(doctorId);
        }

        public async Task<double> GetDoctorAverageRatingAsync(int doctorId)
        {
            return await _reviewRepository.GetAverageRatingAsync(doctorId);
        }

        public async Task<bool> DeleteReviewAsync(int doctorId, int patientId)
        {
            var review = await _reviewRepository.GetReviewAsync(doctorId, patientId);
            if (review == null)
                return false;

            await _reviewRepository.DeleteAsync(review.Id);

            // Update doctor's average rating
            await UpdateDoctorAverageRatingAsync(doctorId);

            return true;
        }

        private async Task UpdateDoctorAverageRatingAsync(int doctorId)
        {
            var doctor = await _doctorRepository.GetByIdAsync(doctorId);
            if (doctor != null)
            {
                doctor.AverageRating = await _reviewRepository.GetAverageRatingAsync(doctorId);
                await _doctorRepository.UpdateAsync(doctor);
            }
        }
    }
}
