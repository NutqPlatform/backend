using Microsoft.EntityFrameworkCore;
using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Infrastructure.Data;

namespace Nutq.Infrastructure.Repositories
{
    public class TransferRequestRepository : Repository<TransferRequest>, ITransferRequestRepository
    {
        public TransferRequestRepository(ApplicationDbContext context) : base(context) { }

        public async Task<TransferRequest?> GetPendingForPatientAndDoctorAsync(int patientId, int toDoctorId)
        {
            return await _context.TransferRequests
                .FirstOrDefaultAsync(t => t.PatientId == patientId && t.ToDoctorId == toDoctorId && t.Status == "Pending");
        }

        public async Task<IEnumerable<TransferRequest>> GetPendingByDoctorIdAsync(int doctorId)
        {
            return await _context.TransferRequests
                .Include(t => t.Patient)
                .Include(t => t.FromDoctor)
                .Where(t => t.ToDoctorId == doctorId && t.Status == "Pending")
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TransferRequest>> GetByPatientIdAsync(int patientId)
        {
            return await _context.TransferRequests
                .Include(t => t.ToDoctor)
                .Include(t => t.FromDoctor)
                .Where(t => t.PatientId == patientId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<TransferRequest?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.TransferRequests
                .Include(t => t.Patient)
                .Include(t => t.FromDoctor)
                .Include(t => t.ToDoctor)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
    }
}
