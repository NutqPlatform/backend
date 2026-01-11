using Nutq.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nutq.Core.Interfaces
{
    public interface IExerciseProgressRepository
    {
        Task AddAsync(ExerciseProgress progress);

        Task<IEnumerable<ExerciseProgress>> GetByPatientAsync(int patientId);

        Task<IEnumerable<ExerciseProgress>> GetByPlanExerciseAsync(int planExerciseId);
    }
}
