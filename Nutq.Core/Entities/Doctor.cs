using System.ComponentModel.DataAnnotations;

namespace Nutq.Core.Entities
{
    public class Doctor
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required, MaxLength(255)]
        public string Password { get; set; } = null!;

        public string? CvText { get; set; }

        [MaxLength(500)]
        public string? CvFileUrl { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public DateTime CreatedAt { get; set; }

        [MaxLength(30)]
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public string? CommunicationInfo { get; set; }

        public string? ProfilePicture { get; set; }

        public bool IsBlocked { get; set; } = false;

        public double AverageRating { get; set; } = 0;

        // Navigation
        public ICollection<Patient>? Patients { get; set; }
        public ICollection<TherapyPlan>? TherapyPlans { get; set; }
        public ICollection<InvitationCode>? InvitationCodes { get; set; }
        public ICollection<WeeklyReport>? WeeklyReports { get; set; }
        public ICollection<DoctorReview>? Reviews { get; set; }
        public ICollection<DoctorPatientRelationship>? PatientRelationships { get; set; }
        public ICollection<TransferRequest>? IncomingTransferRequests { get; set; }
        public ICollection<TransferRequest>? OutgoingTransferRequests { get; set; }
    }
}
