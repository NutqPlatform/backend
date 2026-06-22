using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nutq.Core.Entities
{
    public class SpeechAttempt
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Patient))]
        public int PatientId { get; set; }

        [ForeignKey(nameof(TrainingSession))]
        public int TrainingSessionId { get; set; }

        [ForeignKey(nameof(ExerciseProgress))]
        public int ExerciseProgressId { get; set; }

        [ForeignKey(nameof(Exercise))]
        public int ExerciseId { get; set; }

        [ForeignKey(nameof(Vocabulary))]
        public int? VocabularyId { get; set; }

        [Required, MaxLength(200)]
        public string ExpectedWord { get; set; } = null!;

        [Required, MaxLength(200)]
        public string RecognizedWord { get; set; } = string.Empty;

        public double SimilarityScore { get; set; }

        public int AttemptNumber { get; set; } = 1;

        public bool IsCorrect { get; set; }

        public double AudioDurationSeconds { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        [Required]
        public DateTime AttemptedAt { get; set; }

        public Patient Patient { get; set; } = null!;
        public TrainingSession TrainingSession { get; set; } = null!;
        public ExerciseProgress ExerciseProgress { get; set; } = null!;
        public Exercise Exercise { get; set; } = null!;
        public Vocabulary? Vocabulary { get; set; }
    }
}
