using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nutq.Core.Entities
{
    public class ExerciseProgress
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Patient))]
        public int PatientId { get; set; }

        [ForeignKey(nameof(PlanExercise))]
        public int PlanExerciseId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public double? Score { get; set; }

        public bool Completed { get; set; }

        // Navigation
        public Patient Patient { get; set; } = null!;
        public PlanExercise PlanExercise { get; set; } = null!;
    }
}
