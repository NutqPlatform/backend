using Microsoft.AspNetCore.Mvc;
using Nutq.Core.Interfaces;
using Nutq.Web.DTOs;

namespace Nutq.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorReviewController : ControllerBase
    {
        private readonly IDoctorReviewService _reviewService;
        private readonly IDoctorReviewRepository _reviewRepository;

        public DoctorReviewController(
            IDoctorReviewService reviewService,
            IDoctorReviewRepository reviewRepository)
        {
            _reviewService = reviewService;
            _reviewRepository = reviewRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] DoctorReviewCreateRequest request)
        {
            try
            {
                var review = await _reviewService.CreateReviewAsync(
                    request.DoctorId,
                    request.PatientId,
                    request.Rating,
                    request.Comment);

                var dto = new DoctorReviewDto
                {
                    Id = review.Id,
                    DoctorId = review.DoctorId,
                    PatientId = review.PatientId,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt
                };

                return CreatedAtAction(nameof(GetDoctorReviews), new { doctorId = review.DoctorId }, dto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult> GetDoctorReviews(int doctorId)
        {
            var reviews = await _reviewService.GetDoctorReviewsAsync(doctorId);
            var averageRating = await _reviewService.GetDoctorAverageRatingAsync(doctorId);

            var dto = new DoctorRatingDto
            {
                DoctorId = doctorId,
                AverageRating = averageRating,
                TotalReviews = reviews.Count(),
                Reviews = reviews.Select(r => new DoctorReviewDto
                {
                    Id = r.Id,
                    DoctorId = r.DoctorId,
                    PatientId = r.PatientId,
                    PatientName = r.Patient?.Name,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                }).ToList()
            };

            return Ok(dto);
        }

        [HttpPut("{doctorId}/{patientId}")]
        public async Task<IActionResult> UpdateReview(int doctorId, int patientId, [FromBody] DoctorReviewUpdateRequest request)
        {
            try
            {
                var review = await _reviewService.UpdateReviewAsync(
                    doctorId,
                    patientId,
                    request.Rating,
                    request.Comment);

                if (review == null)
                    return NotFound(new { message = "Review not found" });

                var dto = new DoctorReviewDto
                {
                    Id = review.Id,
                    DoctorId = review.DoctorId,
                    PatientId = review.PatientId,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt
                };

                return Ok(dto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{doctorId}/{patientId}")]
        public async Task<IActionResult> DeleteReview(int doctorId, int patientId)
        {
            var result = await _reviewService.DeleteReviewAsync(doctorId, patientId);
            if (!result)
                return NotFound(new { message = "Review not found" });

            return Ok(new { message = "Review deleted successfully" });
        }
    }
}
