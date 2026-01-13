using Nutq.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nutq.Core.Interfaces
{
    public interface ITherapyPlanRepository : IRepository<TherapyPlan>
    {
        Task<IEnumerable<TherapyPlan>> GetByDoctorAndPatientAsync(int doctorId, int patientId);
       
        Task<List<TherapyPlan>> GetPlansByDoctorAsync(int doctorId);
    }
}
