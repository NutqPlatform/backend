using Nutq.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nutq.Core.Interfaces
{
    public interface ITherapyPlanRepository : IRepository<TherapyPlan>
    {
        Task<IEnumerable<TherapyPlan>> GetByDoctorAndPatientAsync(int doctorId, int patientId);
        Task<List<TherapyPlan>> GetPlansByDoctorAsync(int doctorId);
        Task<List<TherapyPlan>> GetOngoingPlansByDoctorAsync(int doctorId);
        Task<TherapyPlan?> GetPlanWithExercisesByIdAsync(int planId);
        Task<TherapyPlan?> GetPlanWithExercisesForPatientAsync(int planId, int patientId);
        Task<IEnumerable<TherapyPlan>> GetByPatientIdAsync(int patientId);
    }
}
