using Nutq.Core.Entities;
using System.Threading.Tasks;

namespace Nutq.Core.Interfaces
{
    public interface IInvitationCodeRepository : IRepository<InvitationCode>
    {
        Task<InvitationCode?> GetValidCodeAsync(string code, string type);
        Task<bool> ExistsAsync(string code);   // ← ضيفي دي
        Task MarkUsedAsync(int codeId);
    }
}

