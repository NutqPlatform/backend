using Nutq.Core.Entities;
using System.Threading.Tasks;

namespace Nutq.Core.Interfaces
{
    public interface IInvitationCodeRepository
    {
        Task<InvitationCode?> GetValidCodeAsync(string code, string type);
        Task MarkUsedAsync(int codeId);
    }
}
