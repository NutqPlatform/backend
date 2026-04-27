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

        [MaxLength(500)]
        public string? CV { get; set; }

        public string? ProfilePicture { get; set; }

        // Navigation
        public ICollection<Patient>? Patients { get; set; }
        public ICollection<TherapyPlan>? TherapyPlans { get; set; }
        public ICollection<InvitationCode>? InvitationCodes { get; set; }
        public ICollection<WeeklyReport>? WeeklyReports { get; set; }
    }
}
