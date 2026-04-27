using Nutq.Core.Entities;

namespace Nutq.Core.Interfaces
{
    public interface IWeeklyReportRepository : IRepository<WeeklyReport>
    {
        Task<WeeklyReport?> GetByTherapyPlanIdAsync(int planId);
        Task<List<WeeklyReport>> GetByPatientIdAsync(int patientId);
    }
}
