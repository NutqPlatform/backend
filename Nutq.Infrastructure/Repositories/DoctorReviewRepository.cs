using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Nutq.Infrastructure.Repositories
{
    public class DoctorReviewRepository : Repository<DoctorReview>, IDoctorReviewRepository
    {
        public DoctorReviewRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<DoctorReview>> GetReviewsByDoctorIdAsync(int doctorId)
        {
            return await _context.DoctorReviews
                .Where(r => r.DoctorId == doctorId)
                .Include(r => r.Patient)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<DoctorReview?> GetReviewAsync(int doctorId, int patientId)
        {
            return await _context.DoctorReviews
                .FirstOrDefaultAsync(r => r.DoctorId == doctorId && r.PatientId == patientId);
        }

        public async Task<double> GetAverageRatingAsync(int doctorId)
        {
            var reviews = await _context.DoctorReviews
                .Where(r => r.DoctorId == doctorId)
                .ToListAsync();

            if (reviews.Count == 0)
                return 0;

            return Math.Round(reviews.Average(r => r.Rating), 2);
        }
    }
}
