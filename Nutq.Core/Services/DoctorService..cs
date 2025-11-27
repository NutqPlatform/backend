using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Nutq.Core.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IInvitationCodeRepository _codeRepo;
        private readonly IDoctorRepository _doctorRepo;
        private readonly IPatientRepository _patientRepo;
        public DoctorService(IInvitationCodeRepository codeRepo, IDoctorRepository doctorRepo ,IPatientRepository patientRepo)
        {
            _codeRepo = codeRepo;
            _doctorRepo = doctorRepo;
            _patientRepo = patientRepo;
            
        }

        public async Task<string> GeneratePatientCodeAsync(int doctorId)
        {
            var doctor = await _doctorRepo.GetByIdAsync(doctorId);
            if (doctor == null)
                throw new Exception("Doctor not found");

            string code;
            do
            {
                code = GenerateRandomCode();
            }
            while (await _codeRepo.ExistsAsync(code)); // الكود لازم يكون يونيك

            var invitation = new InvitationCode
            {
                Code = code,
                Type = "patient",
                DoctorId = doctorId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMonths(1),
                Used = false
            };

            await _codeRepo.AddAsync(invitation);
            return code;
        }

       private string GenerateRandomCode(int length = 8)
{
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    var bytes = new byte[length];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(bytes);

    char[] result = new char[length];

    for (int i = 0; i < length; i++)
    {
        result[i] = chars[bytes[i] % chars.Length];
    }

    return new string(result);
}
public async Task<IEnumerable<object>> GetDoctorPatientsAsync(int doctorId)
{
    var doctor = await _doctorRepo.GetByIdAsync(doctorId);
    if (doctor == null)
        throw new Exception("Doctor not found");

    var patients = await _patientRepo.GetByDoctorIdAsync(doctorId);

    // هنا بنرجع نسخة خفيفة لتجنب cycles
    return patients.Select(p => new 
    {
        p.Id,
        p.Name,
        p.Email,
        p.Age
    });
}

    }
}
