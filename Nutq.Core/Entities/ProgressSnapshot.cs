using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nutq.Core.Entities
{
    public class ProgressSnapshot
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Patient))]
        public int PatientId { get; set; }

        [ForeignKey(nameof(TrainingSession))]
        public int TrainingSessionId { get; set; }

        [Required]
        public DateTime SnapshotDate { get; set; }

        public int CumulativeTrainingTimeSeconds { get; set; }

        public double AccuracyPercent { get; set; }

        public double FirstAttemptSuccessRate { get; set; }

        public double AverageSimilarity { get; set; }

        public double AverageAttemptsPerWord { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Patient Patient { get; set; } = null!;
        public TrainingSession TrainingSession { get; set; } = null!;
        public ICollection<CategoryPerformanceSnapshot>? CategoryPerformances { get; set; }
    }
}
