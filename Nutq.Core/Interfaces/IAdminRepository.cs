using Nutq.Core.Entities;

namespace Nutq.Core.Interfaces
{
    public interface IAdminRepository : IRepository<Admin>
    {
        Task<Admin?> GetByEmailAsync(string email);
    }
}
