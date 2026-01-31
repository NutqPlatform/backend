
using Nutq.Core.Commands;
using Nutq.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nutq.Core.Interfaces
{
    public interface IExerciseProgressService
    {
        Task AddOrUpdateProgressAsync(ExerciseProgressCommand command);
        Task<IEnumerable<ExerciseProgress>> GetPatientProgressAsync(int patientId);
        Task StartExerciseAsync(int patientId, int planExerciseId);
        Task CompleteExerciseAsync(int patientId, int planExerciseId);
    }
}
