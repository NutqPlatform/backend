using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nutq.Core.Entities
{
    public class Exercise
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(DifficultyLevel))]
        public int DifficultyId { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Difficulty { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        public string? Tags { get; set; }

        public string? AssetUrl { get; set; }

        // Navigation
        public DifficultyLevel DifficultyLevel { get; set; } = null!;
        public ICollection<PlanExercise>? PlanExercises { get; set; }
    }
}
