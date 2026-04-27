using Microsoft.EntityFrameworkCore;
using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nutq.Infrastructure.Repositories
{
    public class VocabularyRepository : Repository<Vocabulary>, IVocabularyRepository
    {
        public VocabularyRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Vocabulary>> GetByCategoryAndDifficultyLevelAsync(string category, string difficultyLevelName)
        {
            return await _context.Vocabularies
                .Include(v => v.DifficultyLevel)
                .Where(v => v.Category == category &&
                    (v.DifficultyLevel.Name == difficultyLevelName || v.DifficultyLevel.Level == difficultyLevelName))
                .ToListAsync();
        }
    }
}
