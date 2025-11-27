using System.Threading.Tasks;
using Nutq.Core.Entities;

namespace Nutq.Core.Interfaces
{
    public interface IDoctorService
{
    Task<string> GeneratePatientCodeAsync(int doctorId);
    
    Task<IEnumerable<object>> GetDoctorPatientsAsync(int doctorId); // بدل Patient
}

}
