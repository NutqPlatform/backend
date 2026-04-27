using Nutq.Core.Entities;

namespace Nutq.Core.Interfaces
{
    public interface IExerciseRepository : IRepository<Exercise>
    {
        Task<IEnumerable<Exercise>> GetAllAsync();
        Task<Exercise?> GetByIdAsync(int id);
        Task<IEnumerable<Exercise>> GetByDifficultyAsync(int difficultyId);
    }
}
