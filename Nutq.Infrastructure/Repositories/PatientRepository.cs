using System;
using Microsoft.EntityFrameworkCore;
using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Infrastructure.Data;

namespace Nutq.Infrastructure.Repositories
{
    public class PatientRepository : Repository<Patient>, IPatientRepository
    {
        public PatientRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<Patient?> GetByEmailAsync(string email)
        {
            return await _context.Patients
                .FirstOrDefaultAsync(p => p.Email == email);
        }

        public async Task<IEnumerable<Patient>> GetByDoctorIdAsync(int doctorId)
        {
            return await _context.Patients
                .Where(p => p.DoctorId == doctorId)
                .ToListAsync();
        }

        public async Task<List<TherapyPlan>> GetPatientPlansAsync(int patientId)
        {
            var plans = await _context.TherapyPlans
                .Where(p => p.PatientId == patientId)
                .Include(p => p.PlanExercises!)
                    .ThenInclude(pe => pe.Exercise)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();

            var now = DateTime.UtcNow;
            var updated = false;

            foreach (var plan in plans)
            {
                if (plan.EndDate.HasValue && plan.EndDate.Value <= now && plan.Status != "Completed")
                {
                    plan.Status = "Completed";
                    _context.TherapyPlans.Update(plan);
                    updated = true;
                }
            }

            if (updated)
            {
                await _context.SaveChangesAsync();
            }

            return plans;
        }

        public async Task<List<ExerciseProgress>> GetPatientProgressAsync(int patientId)
        {
            return await _context.ExerciseProgresses
                .Where(p => p.PatientId == patientId)
                .Include(p => p.PlanExercise!)
                    .ThenInclude(pe => pe.Exercise)
                .ToListAsync();
        }
    }
}
