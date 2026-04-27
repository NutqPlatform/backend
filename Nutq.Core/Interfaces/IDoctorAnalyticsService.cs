using Nutq.Core.Entities;

namespace Nutq.Core.Interfaces
{
    public interface IDoctorAnalyticsService
    {
        Task<int> GetTotalPatientsAsync(int doctorId);
        Task<int> GetTotalPlansAsync(int doctorId);
        Task<int> GetTotalExercisesAsync(int doctorId);
        Task<double> GetAverageCompletionRateAsync(int doctorId);
    }
}
