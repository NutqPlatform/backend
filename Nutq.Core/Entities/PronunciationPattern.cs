using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nutq.Core.Entities
{
    /// <summary>
    /// Recurring speech error pattern detected across sessions.
    /// MetadataJson reserved for future AI phoneme analysis, confidence scores, model version, etc.
    /// </summary>
    public class PronunciationPattern
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Patient))]
        public int PatientId { get; set; }

        [ForeignKey(nameof(Vocabulary))]
        public int? VocabularyId { get; set; }

        [Required, MaxLength(200)]
        public string ExpectedPattern { get; set; } = null!;

        [Required, MaxLength(200)]
        public string RecognizedPattern { get; set; } = null!;

        /// <summary>Substitution | Omission | Distortion | Addition | Other</summary>
        [Required, MaxLength(50)]
        public string PatternType { get; set; } = "Substitution";

        [MaxLength(100)]
        public string? Category { get; set; }

        public int OccurrenceCount { get; set; } = 1;

        public double AverageSimilarityScore { get; set; }

        /// <summary>0-100 severity derived from similarity and frequency</summary>
        public double SeverityScore { get; set; }

        /// <summary>Deterministic | Ai</summary>
        [Required, MaxLength(30)]
        public string AnalysisSource { get; set; } = "Deterministic";

        /// <summary>Future AI fields: phonemeErrors, confidence, modelVersion, acousticFeatures, etc.</summary>
        public string? MetadataJson { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime FirstDetectedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastDetectedAt { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Patient Patient { get; set; } = null!;
        public Vocabulary? Vocabulary { get; set; }
    }
}
