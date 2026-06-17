using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Nutq.Infrastructure.Repositories
{
    public class VocabularyExerciseRepository : Repository<VocabularyExercise>, IVocabularyExerciseRepository
    {
        public VocabularyExerciseRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<VocabularyExercise>> GetByExerciseIdAsync(int exerciseId)
        {
            return await _context.VocabularyExercises
                .Where(ve => ve.ExerciseId == exerciseId)
                .Include(ve => ve.Vocabulary)
                .Include(ve => ve.DifficultyLevel)
                .ToListAsync();
        }

        public async Task<IEnumerable<VocabularyExercise>> GetByDifficultyLevelAsync(int difficultyLevelId)
        {
            return await _context.VocabularyExercises
                .Where(ve => ve.DifficultyLevelId == difficultyLevelId)
                .Include(ve => ve.Vocabulary)
                .Include(ve => ve.Exercise)
                .ToListAsync();
        }
    }
}
