using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nutq.Core.Entities
{
    public class SessionClinicalReport
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Patient))]
        public int PatientId { get; set; }

        [ForeignKey(nameof(TrainingSession))]
        public int TrainingSessionId { get; set; }

        public int SessionDurationSeconds { get; set; }

        public int ExercisesCompleted { get; set; }

        public double AccuracyRate { get; set; }

        public double FirstAttemptSuccessRate { get; set; }

        public double AveragePronunciationSimilarity { get; set; }

        /// <summary>JSON array of category/area names</summary>
        public string? StrengthAreasJson { get; set; }

        /// <summary>JSON array of category/area names</summary>
        public string? WeaknessAreasJson { get; set; }

        /// <summary>JSON array of recommended focus areas with rationale</summary>
        public string? RecommendedFocusJson { get; set; }

        /// <summary>Deterministic | Ai</summary>
        [Required, MaxLength(30)]
        public string AnalysisSource { get; set; } = "Deterministic";

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        public Patient Patient { get; set; } = null!;
        public TrainingSession TrainingSession { get; set; } = null!;
    }
}
