using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nutq.Core.Entities
{
    public class Patient
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Doctor))]
        public int DoctorId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required, MaxLength(255)]
        public string Password { get; set; } = null!;

        public DateTime? DateOfBirth { get; set; }

        public DateTime CreatedAt { get; set; }

        [MaxLength(30)]
        public string? PhoneNumber { get; set; }

        public string? DiagnosisText { get; set; }

        public string? DiagnosisFileUrl { get; set; }

        public string? ProfilePicture { get; set; }

        public bool IsBlocked { get; set; } = false;

        // Navigation
        public Doctor Doctor { get; set; } = null!;
        public ICollection<TherapyPlan>? TherapyPlans { get; set; }
        public ICollection<WeeklyReport>? WeeklyReports { get; set; }
        public ICollection<ExerciseProgress>? ExerciseProgressRecords { get; set; }
        public ICollection<DoctorReview>? DoctorReviews { get; set; }
    }
}
