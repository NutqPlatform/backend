using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nutq.Core.Entities
{
    public class PlanExercise
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(TherapyPlan))]
        public int TherapyPlanId { get; set; }

        [ForeignKey(nameof(Exercise))]
        public int ExerciseId { get; set; }

        [Range(1, 600)]
        public int DurationMinutes { get; set; }

        [Range(1, 100)]
        public int Repetition { get; set; }

        public string? AiConstraints { get; set; }

        // Navigation
        public TherapyPlan TherapyPlan { get; set; } = null!;
        public Exercise Exercise { get; set; } = null!;
        public ICollection<ExerciseProgress>? ExerciseProgressRecords { get; set; }
    }
}
