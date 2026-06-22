using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nutq.Core.Entities
{
    public class TrainingSession
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Patient))]
        public int PatientId { get; set; }

        [ForeignKey(nameof(ExerciseProgress))]
        public int ExerciseProgressId { get; set; }

        [ForeignKey(nameof(Exercise))]
        public int ExerciseId { get; set; }

        [ForeignKey(nameof(PlanExercise))]
        public int PlanExerciseId { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public int TotalDurationSeconds { get; set; }

        public int WordsCompleted { get; set; }

        public int FirstAttemptCorrectCount { get; set; }

        public double AverageSimilarityScore { get; set; }

        public double AccuracyPercent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Patient Patient { get; set; } = null!;
        public ExerciseProgress ExerciseProgress { get; set; } = null!;
        public Exercise Exercise { get; set; } = null!;
        public PlanExercise PlanExercise { get; set; } = null!;
        public ICollection<SpeechAttempt>? SpeechAttempts { get; set; }
        public ProgressSnapshot? ProgressSnapshot { get; set; }
        public SessionClinicalReport? ClinicalReport { get; set; }
    }
}
