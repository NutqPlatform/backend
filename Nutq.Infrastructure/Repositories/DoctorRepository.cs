using Microsoft.EntityFrameworkCore;
using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Infrastructure.Data;

namespace Nutq.Infrastructure.Repositories
{
     public class DoctorRepository : Repository<Doctor>, IDoctorRepository
    {
       

        public DoctorRepository(ApplicationDbContext context) : base(context)
        {
            
        }
         public async Task<Doctor?> GetByEmailAsync(string email)
        {
            return await _context.Doctors.FirstOrDefaultAsync(d => d.Email == email);
        }
        public async Task<List<Patient>> GetPatientsAsync(int doctorId)
        {
            return await _context.Patients
                .Where(p => p.DoctorId == doctorId)
                .ToListAsync();
        }

        public async Task<List<Doctor>> GetAllWithPatientsAndReportsAsync()
        {
            return await _context.Doctors
                .Include(d => d.Patients)
                .Include(d => d.WeeklyReports!)
                    .ThenInclude(w => w.Patient)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
