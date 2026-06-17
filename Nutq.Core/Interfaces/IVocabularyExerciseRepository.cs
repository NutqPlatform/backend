using Nutq.Core.Entities;

namespace Nutq.Core.Interfaces
{
    public interface IVocabularyExerciseRepository : IRepository<VocabularyExercise>
    {
        Task<IEnumerable<VocabularyExercise>> GetByExerciseIdAsync(int exerciseId);
        Task<IEnumerable<VocabularyExercise>> GetByDifficultyLevelAsync(int difficultyLevelId);
    }
}
