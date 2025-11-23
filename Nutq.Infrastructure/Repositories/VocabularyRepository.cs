using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Infrastructure.Data;

namespace Nutq.Infrastructure.Repositories
{
    public class VocabularyRepository : Repository<Vocabulary>, IVocabularyRepository
    {
        public VocabularyRepository(ApplicationDbContext context) : base(context)
        {
        }

        
    }
}
