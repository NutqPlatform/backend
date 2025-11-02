using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nutq.Core.Entities
{
    public class Vocabulary
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string WordArabic { get; set; } = null!;

        [Required, MaxLength(100)]
        public string WordEnglish { get; set; } = null!;

        [ForeignKey(nameof(DifficultyLevel))]
        public int DifficultyLevelId { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        public string? ImageUrl { get; set; }
        public string? SoundUrl { get; set; }
        public string? VideoUrl { get; set; }

        public string? ImageDescriptions { get; set; }
        public string? Tags { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        // Navigation
        public DifficultyLevel DifficultyLevel { get; set; } = null!;
    }
}
