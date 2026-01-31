using Nutq.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nutq.Core.Interfaces
{
    public interface IVocabularyRepository : IRepository<Vocabulary>
    {
        Task<IEnumerable<Vocabulary>> GetByCategoryAndDifficultyLevelAsync(string category, string difficultyLevelName);
    }
}
