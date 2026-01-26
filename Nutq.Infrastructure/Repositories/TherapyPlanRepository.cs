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
    

        public TherapyPlanRepository(ApplicationDbContext context) : base(context)
        {
            
        }

        public async Task<List<TherapyPlan>> GetPlansByDoctorAsync(int doctorId)
        {
            return await _context.TherapyPlans
                .Where(p => p.DoctorId == doctorId)
                .Include(tp => tp.Patient)
                .Include(tp => tp.PlanExercises!)
                    .ThenInclude(pe => pe.Exercise)
                .ToListAsync();
        }
         public async Task<IEnumerable<TherapyPlan>> GetByDoctorAndPatientAsync(int doctorId, int patientId)
        {
            return await _context.TherapyPlans
                .Where(tp => tp.DoctorId == doctorId && tp.PatientId == patientId)
                .Include(tp => tp.PlanExercises!)
                    .ThenInclude(pe => pe.Exercise)
                .ToListAsync();
        }
    }
       
}

