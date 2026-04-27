using Microsoft.EntityFrameworkCore;
using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Infrastructure.Data;

namespace Nutq.Infrastructure.Repositories
{
    public class ExerciseRepository 
        : Repository<Exercise>, IExerciseRepository
    {
        public ExerciseRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<Exercise>> GetAllAsync()
        {
            return await _context.Exercises
                .Include(e => e.DifficultyLevel)
                .ToListAsync();
        }

        public async Task<Exercise?> GetByIdAsync(int id)
        {
            return await _context.Exercises
                .Include(e => e.DifficultyLevel)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Exercise>> GetByDifficultyAsync(int difficultyId)
        {
            return await _context.Exercises
                .Include(e => e.DifficultyLevel)
                .Where(e => e.DifficultyId == difficultyId)
                .ToListAsync();
        }
    }
}
