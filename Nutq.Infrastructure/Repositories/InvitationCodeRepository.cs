using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Nutq.Infrastructure.Repositories
{
    public class InvitationCodeRepository : Repository<InvitationCode>, IInvitationCodeRepository
    {
        public InvitationCodeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<InvitationCode?> GetValidCodeAsync(string code, string type)
        {
            return await _context.InvitationCodes
                .FirstOrDefaultAsync(c =>
                    c.Code == code &&
                    c.Type == type &&
                    !c.Used &&
                    c.ExpireAt > DateTime.UtcNow);
        }

        public async Task MarkUsedAsync(int codeId)
        {
            var code = await _context.InvitationCodes.FindAsync(codeId);
            if (code != null)
            {
                code.Used = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
