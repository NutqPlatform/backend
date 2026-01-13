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
            return await _context.TherapyPlans
                .Where(p => p.PatientId == patientId)
                .Include(p => p.PlanExercises!)
                    .ThenInclude(pe => pe.Exercise)
                .ToListAsync();
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
