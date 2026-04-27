
using Nutq.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nutq.Core.Interfaces
{
    public interface IExerciseProgressRepository
    {
        Task AddAsync(ExerciseProgress progress);
        Task UpdateAsync(ExerciseProgress progress);

        Task<ExerciseProgress?> GetByPatientAndPlanExerciseAsync(int patientId, int planExerciseId);
        Task<IEnumerable<ExerciseProgress>> GetByPatientAsync(int patientId);
        Task<IEnumerable<ExerciseProgress>> GetByPlanExerciseAsync(int planExerciseId);
        Task<List<ExerciseProgress>> GetByPlanIdsAsync(List<int> planIds);
        Task<List<ExerciseProgress>> GetByPlanExerciseIdsAsync(List<int> planExerciseIds);
    }
}
