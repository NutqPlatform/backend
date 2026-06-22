using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nutq.Core.Entities
{
    public class CategoryPerformanceSnapshot
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(ProgressSnapshot))]
        public int ProgressSnapshotId { get; set; }

        [ForeignKey(nameof(Patient))]
        public int PatientId { get; set; }

        [Required, MaxLength(100)]
        public string Category { get; set; } = null!;

        public double AccuracyPercent { get; set; }

        public double AverageSimilarity { get; set; }

        public double AverageAttemptsPerWord { get; set; }

        public int WordsAttempted { get; set; }

        /// <summary>Improving | Declining | Stable | InsufficientData</summary>
        [Required, MaxLength(30)]
        public string TrendDirection { get; set; } = "InsufficientData";

        public double? PreviousAccuracyPercent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ProgressSnapshot ProgressSnapshot { get; set; } = null!;
        public Patient Patient { get; set; } = null!;
    }
}
