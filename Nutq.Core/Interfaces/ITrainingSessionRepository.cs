using Nutq.Core.Entities;
using Nutq.Core.Models;

namespace Nutq.Core.Interfaces
{
    public interface ITrainingSessionRepository
    {
        Task<TrainingSession> AddAsync(TrainingSession session);
        Task<TrainingSession?> GetByExerciseProgressIdAsync(int exerciseProgressId);
        Task<TrainingSession?> GetByPatientAndPlanExerciseAsync(int patientId, int planExerciseId);
        Task<TrainingSession?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<TrainingSession>> GetByPatientAsync(int patientId, DateTime? from = null, DateTime? to = null);
        Task<int> GetTotalDurationSecondsAsync(int patientId);
        Task<IReadOnlyList<PatientSessionTimelineProjection>> GetSessionTimelineProjectionsAsync(int patientId);
        Task<IEnumerable<TrainingSession>> GetByPlanExerciseIdsAsync(IEnumerable<int> planExerciseIds);
    }
}
