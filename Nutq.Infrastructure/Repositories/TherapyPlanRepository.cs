using Microsoft.EntityFrameworkCore;
using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nutq.Infrastructure.Repositories
{
    public class TherapyPlanRepository : Repository<TherapyPlan>, ITherapyPlanRepository
    {
        private readonly ApplicationDbContext _context;
        public TherapyPlanRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TherapyPlan>> GetByDoctorAndPatientAsync(int doctorId, int patientId)
        {
            return await _context.TherapyPlans
                .Where(tp => tp.DoctorId == doctorId && tp.PatientId == patientId)
                .Include(tp => tp.PlanExercises)
                    .ThenInclude(pe => pe.Exercise)
                .ToListAsync();
        }
    }
}
