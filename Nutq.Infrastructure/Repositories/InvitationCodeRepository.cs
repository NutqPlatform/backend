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
    .FirstOrDefaultAsync(x =>
        x.Code.ToLower() == code.ToLower() &&
        x.Type.ToLower() == type.ToLower() &&
        !x.Used &&
        (x.ExpiresAt == null || x.ExpiresAt > DateTime.UtcNow));

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
        public async Task<bool> ExistsAsync(string code)
{
    return await _context.InvitationCodes.AnyAsync(c => c.Code == code);
}

    }
}
