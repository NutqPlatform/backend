using Nutq.Core.Entities;
using Nutq.Core.Interfaces;

namespace Nutq.Core.Services
{
    public class ExerciseService : IExerciseService
    {
        private readonly IExerciseRepository _exerciseRepo;

        public ExerciseService(IExerciseRepository exerciseRepo)
        {
            _exerciseRepo = exerciseRepo;
        }

        public async Task<IEnumerable<Exercise>> GetAllAsync()
        {
            return await _exerciseRepo.GetAllAsync();
        }

        public async Task<Exercise?> GetByIdAsync(int id)
        {
            return await _exerciseRepo.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Exercise>> GetByDifficultyAsync(int difficultyId)
        {
            return await _exerciseRepo.GetByDifficultyAsync(difficultyId);
        }
    }
}
