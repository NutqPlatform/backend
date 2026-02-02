using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nutq.Core.Entities
{
    public class WeeklyReport
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Doctor))]
        public int DoctorId { get; set; }

        [ForeignKey(nameof(Patient))]
        public int PatientId { get; set; }

        [ForeignKey(nameof(TherapyPlan))]
        public int? TherapyPlanId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public double TotalHours { get; set; }

        public string? AiSummary { get; set; }

        public string? DoctorNotes { get; set; }

        // Navigation
        public Doctor Doctor { get; set; } = null!;
        public Patient Patient { get; set; } = null!;
        public TherapyPlan? TherapyPlan { get; set; }
    }
}
