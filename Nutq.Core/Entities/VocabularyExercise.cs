using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nutq.Core.Entities
{
    public class VocabularyExercise
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Vocabulary))]
        public int VocabularyId { get; set; }

        [ForeignKey(nameof(Exercise))]
        public int ExerciseId { get; set; }

        [ForeignKey(nameof(DifficultyLevel))]
        public int DifficultyLevelId { get; set; }

        // Navigation
        public Vocabulary Vocabulary { get; set; } = null!;
        public Exercise Exercise { get; set; } = null!;
        public DifficultyLevel DifficultyLevel { get; set; } = null!;
    }
}
