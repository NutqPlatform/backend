using Nutq.Core.Entities;

namespace Nutq.Core.Interfaces
{
    public interface IDoctorRepository : IRepository<Doctor>
    {
        Task<Doctor?> GetByEmailAsync(string email);
        Task<List<Patient>> GetPatientsAsync(int doctorId);

        Task<List<Doctor>> GetAllWithPatientsAndReportsAsync();
    }
}
