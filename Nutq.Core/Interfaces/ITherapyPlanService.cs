using Nutq.Core.Entities;
using Nutq.Core.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nutq.Core.Interfaces
{
    public interface ITherapyPlanService
    {
        Task<TherapyPlan> CreatePlanAsync(int doctorId, int patientId, CreateTherapyPlanCommand command);
        Task<IEnumerable<PlanExercise>> AddExerciseToPlanAsync(int planId, AddPlanExerciseCommand command);
        Task<IEnumerable<TherapyPlan>> GetPlansForPatientAsync(int doctorId, int patientId);
        Task<TherapyPlan?> GetPlanByIdAsync(int planId);
        Task<TherapyPlan?> GetPlanWithExercisesForPatientAsync(int planId, int patientId);
        Task DeleteExerciseFromPlanAsync(int planId, int planExerciseId);
        Task UpdatePlanStatusAsync(int planId, string status);
        Task<IEnumerable<TherapyPlan>> GetActivePlansForDoctorAsync(int doctorId);
        Task<IEnumerable<TherapyPlan>> GetPlansByDoctorAsync(int doctorId);

    }
}
