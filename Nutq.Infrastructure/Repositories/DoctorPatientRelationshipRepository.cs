using Microsoft.EntityFrameworkCore;
using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Infrastructure.Data;

namespace Nutq.Infrastructure.Repositories
{
    public class DoctorPatientRelationshipRepository : Repository<DoctorPatientRelationship>, IDoctorPatientRelationshipRepository
    {
        public DoctorPatientRelationshipRepository(ApplicationDbContext context) : base(context) { }

        public async Task<DoctorPatientRelationship?> GetActiveAsync(int doctorId, int patientId)
        {
            return await _context.DoctorPatientRelationships
                .FirstOrDefaultAsync(r => r.DoctorId == doctorId && r.PatientId == patientId && r.EndedAt == null);
        }

        public async Task<DoctorPatientRelationship?> GetActiveForPatientAsync(int patientId)
        {
            return await _context.DoctorPatientRelationships
                .Include(r => r.Doctor)
                .FirstOrDefaultAsync(r => r.PatientId == patientId && r.EndedAt == null);
        }

        public async Task<IEnumerable<DoctorPatientRelationship>> GetActiveByDoctorIdAsync(int doctorId)
        {
            return await _context.DoctorPatientRelationships
                .Include(r => r.Patient)
                .Where(r => r.DoctorId == doctorId && r.EndedAt == null)
                .ToListAsync();
        }

        public async Task<IEnumerable<DoctorPatientRelationship>> GetFormerByDoctorIdAsync(int doctorId)
        {
            return await _context.DoctorPatientRelationships
                .Include(r => r.Patient)
                .Where(r => r.DoctorId == doctorId && r.EndedAt != null)
                .OrderByDescending(r => r.EndedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<DoctorPatientRelationship>> GetByDoctorIdAsync(int doctorId)
        {
            return await _context.DoctorPatientRelationships
                .Where(r => r.DoctorId == doctorId)
                .ToListAsync();
        }

        public async Task<IEnumerable<DoctorPatientRelationship>> GetByPatientIdAsync(int patientId)
        {
            return await _context.DoctorPatientRelationships
                .Where(r => r.PatientId == patientId)
                .OrderByDescending(r => r.AssignedAt)
                .ToListAsync();
        }

        public async Task<DoctorPatientRelationship?> GetLastEndedForPatientAsync(int patientId)
        {
            return await _context.DoctorPatientRelationships
                .Where(r => r.PatientId == patientId && r.EndedAt != null)
                .OrderByDescending(r => r.EndedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> HasRelationshipAsync(int doctorId, int patientId)
        {
            return await _context.DoctorPatientRelationships
                .AnyAsync(r => r.DoctorId == doctorId && r.PatientId == patientId);
        }

        public async Task<int> CountDistinctPatientsAsync(int doctorId)
        {
            return await _context.DoctorPatientRelationships
                .Where(r => r.DoctorId == doctorId)
                .Select(r => r.PatientId)
                .Distinct()
                .CountAsync();
        }
    }
}
