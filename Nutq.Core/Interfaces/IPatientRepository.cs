using Nutq.Core.Entities;

namespace Nutq.Core.Interfaces
{
    public interface IPatientRepository : IRepository<Patient>
{
    Task<Patient?> GetByEmailAsync(string email);
}

}
