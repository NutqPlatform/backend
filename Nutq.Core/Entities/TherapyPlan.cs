using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nutq.Core.Entities
{
    public class TherapyPlan
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Doctor))]
        public int DoctorId { get; set; }

        [ForeignKey(nameof(Patient))]
        public int PatientId { get; set; }

        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        // Navigation
        public Doctor Doctor { get; set; } = null!;
        public Patient Patient { get; set; } = null!;
        public ICollection<PlanExercise>? PlanExercises { get; set; }
    }
}
