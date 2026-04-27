using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using System;
using System.Linq;
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
            while (await _codeRepo.ExistsAsync(code)); // unique

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

public async Task<object?> GetPatientByIdAsync(int doctorId, int patientId)
{
    var doctor = await _doctorRepo.GetByIdAsync(doctorId);
    if (doctor == null)
        throw new Exception("Doctor not found");

    var patient = await _patientRepo.GetByIdAsync(patientId);
    if (patient == null)
        throw new Exception($"Patient with ID {patientId} not found");
    
    if (patient.DoctorId != doctorId)
        throw new Exception($"Patient with ID {patientId} does not belong to doctor {doctorId}");

    return new
    {
        patient.Id,
        patient.Name,
        patient.Email,
        patient.Age,
        patient.Diagnosis,
        patient.ProfilePicture
    };
}

public async Task UpdatePatientDiagnosisAsync(int doctorId, int patientId, string diagnosis)
{
    var doctor = await _doctorRepo.GetByIdAsync(doctorId);
    if (doctor == null)
        throw new Exception("Doctor not found");

    var patient = await _patientRepo.GetByIdAsync(patientId);
    if (patient == null || patient.DoctorId != doctorId)
        throw new Exception("Patient not found or does not belong to this doctor");

    patient.Diagnosis = diagnosis;
    await _patientRepo.UpdateAsync(patient);
}

public async Task<object> GetDoctorProfileAsync(int doctorId)
{
    var doctor = await _doctorRepo.GetByIdAsync(doctorId);
    if (doctor == null)
        throw new Exception("Doctor not found");

    return new
    {
        doctor.Id,
        doctor.Name,
        doctor.Email,
        doctor.ProfilePicture,
        doctor.CV
    };
}

public async Task UpdateDoctorProfileAsync(int doctorId, string? profilePicture, string? cv)
{
    var doctor = await _doctorRepo.GetByIdAsync(doctorId);
    if (doctor == null)
        throw new Exception("Doctor not found");

    if (profilePicture != null)
        doctor.ProfilePicture = profilePicture;
    
    if (cv != null)
        doctor.CV = cv;

    await _doctorRepo.UpdateAsync(doctor);
}

public async Task UpdateDoctorPasswordAsync(int doctorId, string currentPassword, string newPassword)
{
    var doctor = await _doctorRepo.GetByIdAsync(doctorId);
    if (doctor == null)
        throw new Exception("Doctor not found");

    if (doctor.Password != currentPassword)
        throw new Exception("Current password is incorrect");

    doctor.Password = newPassword;
    await _doctorRepo.UpdateAsync(doctor);
}

public async Task<IEnumerable<object>> GetAllDoctorsWithCommunicationsAsync()
{
    var doctors = await _doctorRepo.GetAllWithPatientsAndReportsAsync();

    return doctors.Select(d => new
    {
        d.Id,
        d.Name,
        d.Email,
        d.ProfilePicture,
        d.CV,
        Patients = d.Patients?.Select(p => new
        {
            p.Id,
            p.Name,
            p.Email,
            p.Age,
            p.ProfilePicture
        }) ?? Enumerable.Empty<object>(),
        WeeklyReports = d.WeeklyReports?.Select(r => new
        {
            r.Id,
            r.PatientId,
            PatientName = r.Patient?.Name,
            r.StartDate,
            r.EndDate,
            r.TotalHours,
            r.DoctorNotes,
            r.AiSummary
        }) ?? Enumerable.Empty<object>()
    });
}

public async Task<object> GetDoctorWithCommunicationsAsync(int doctorId)
{
    var doctors = await _doctorRepo.GetAllWithPatientsAndReportsAsync();
    var doctor = doctors.FirstOrDefault(d => d.Id == doctorId);
    if (doctor == null)
        throw new Exception("Doctor not found");

    return new
    {
        doctor.Id,
        doctor.Name,
        doctor.Email,
        doctor.ProfilePicture,
        doctor.CV,
        Patients = doctor.Patients?.Select(p => new
        {
            p.Id,
            p.Name,
            p.Email,
            p.Age,
            p.ProfilePicture
        }) ?? Enumerable.Empty<object>(),
        WeeklyReports = doctor.WeeklyReports?.Select(r => new
        {
            r.Id,
            r.PatientId,
            PatientName = r.Patient?.Name,
            r.StartDate,
            r.EndDate,
            r.TotalHours,
            r.DoctorNotes,
            r.AiSummary
        }) ?? Enumerable.Empty<object>()
    };
}

    }
}
