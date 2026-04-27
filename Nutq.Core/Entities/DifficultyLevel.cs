using System.ComponentModel.DataAnnotations;

namespace Nutq.Core.Entities
{
    public class DifficultyLevel
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Level { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        // Navigation
        public ICollection<Exercise>? Exercises { get; set; }
        public ICollection<Vocabulary>? Vocabularies { get; set; }
    }
}
