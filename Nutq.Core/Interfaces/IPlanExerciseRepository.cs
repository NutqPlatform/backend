using Nutq.Core.Entities;
using System.Threading.Tasks;

namespace Nutq.Core.Interfaces
{
    public interface IPlanExerciseRepository : IRepository<PlanExercise>
    {
        Task<List<PlanExercise>> GetByPlanIdsAsync(List<int> planIds);
    }
}
