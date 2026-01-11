using Nutq.Core.Commands;
using Nutq.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nutq.Core.Interfaces
{
    public interface IExerciseProgressService
    {
        Task AddProgressAsync(ExerciseProgressCommand command);

        Task<IEnumerable<ExerciseProgress>> GetPatientProgressAsync(int patientId);
    }
}
