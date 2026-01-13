using Nutq.Core.Entities;

namespace Nutq.Core.Interfaces
{
    public interface IPatientDashboardService
    {
        Task<List<TherapyPlan>> GetPatientPlansAsync(int patientId);
        Task<List<ExerciseProgress>> GetPatientProgressAsync(int patientId);
    }
}
