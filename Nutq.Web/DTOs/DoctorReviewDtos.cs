namespace Nutq.Web.DTOs
{
    public class DoctorReviewCreateRequest
    {
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }

    public class DoctorReviewUpdateRequest
    {
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }

    public class DoctorReviewDto
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public string? PatientName { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DoctorRatingDto
    {
        public int DoctorId { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public List<DoctorReviewDto> Reviews { get; set; } = new();
    }
}
