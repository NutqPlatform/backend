using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using Nutq.Core.Auth;

namespace Nutq.Core.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;

        public AdminService(
            IAdminRepository adminRepository,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository)
        {
            _adminRepository = adminRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            var admin = await _adminRepository.GetByEmailAsync(email);
            
            if (admin == null || admin.Password != password || !admin.IsActive)
            {
                return new AuthResult { Success = false, Message = "Invalid credentials or account inactive" };
            }

            var token = JwtTokenGenerator.GenerateToken(admin.Id, admin.Email, "Admin", admin.Name);
            var expires = JwtTokenGenerator.GetExpirationTime();

            return new AuthResult
            {
                Success = true,
                Token = token,
                Expires = expires,
                UserId = admin.Id,
                Email = admin.Email,
                Name = admin.Name,
                Role = "Admin",
                Message = "Login successful"
            };
        }

        public async Task<bool> BlockDoctorAsync(int doctorId)
        {
            var doctor = await _doctorRepository.GetByIdAsync(doctorId);
            if (doctor == null)
                return false;

            doctor.IsBlocked = true;
            await _doctorRepository.UpdateAsync(doctor);
            return true;
        }

        public async Task<bool> UnblockDoctorAsync(int doctorId)
        {
            var doctor = await _doctorRepository.GetByIdAsync(doctorId);
            if (doctor == null)
                return false;

            doctor.IsBlocked = false;
            await _doctorRepository.UpdateAsync(doctor);
            return true;
        }

        public async Task<bool> BlockPatientAsync(int patientId)
        {
            var patient = await _patientRepository.GetByIdAsync(patientId);
            if (patient == null)
                return false;

            patient.IsBlocked = true;
            await _patientRepository.UpdateAsync(patient);
            return true;
        }

        public async Task<bool> UnblockPatientAsync(int patientId)
        {
            var patient = await _patientRepository.GetByIdAsync(patientId);
            if (patient == null)
                return false;

            patient.IsBlocked = false;
            await _patientRepository.UpdateAsync(patient);
            return true;
        }
    }
}
