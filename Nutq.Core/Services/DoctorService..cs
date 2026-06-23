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
        private readonly IDoctorPatientRelationshipRepository _relationshipRepo;

        public DoctorService(IInvitationCodeRepository codeRepo, IDoctorRepository doctorRepo, IPatientRepository patientRepo, IDoctorPatientRelationshipRepository relationshipRepo)
        {
            _codeRepo = codeRepo;
            _doctorRepo = doctorRepo;
            _patientRepo = patientRepo;
            _relationshipRepo = relationshipRepo;
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

    var activeRelationships = await _relationshipRepo.GetActiveByDoctorIdAsync(doctorId);
    var patients = activeRelationships.Select(r => r.Patient).Where(p => p != null).ToList();

    return patients.Select(p => new 
    {
        p.Id,
        p.Name,
        p.Email,
        dateOfBirth = p.DateOfBirth,
        phoneNumber = p.PhoneNumber,
        age = p.DateOfBirth.HasValue ? (int?)((DateTime.UtcNow - p.DateOfBirth.Value).TotalDays / 365.2425) : (int?)null,
        profilePicture = p.ProfilePicture,
        diagnosis = p.DiagnosisText,
        createdAt = p.CreatedAt
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

    var active = await _relationshipRepo.GetActiveAsync(doctorId, patientId);
    var isCurrent = active != null;
    if (!isCurrent)
    {
        var ended = (await _relationshipRepo.GetFormerByDoctorIdAsync(doctorId))
            .FirstOrDefault(r => r.PatientId == patientId);
        if (ended == null)
            throw new Exception($"Patient with ID {patientId} does not belong to doctor {doctorId}");

        return new
        {
            patient.Id,
            patient.Name,
            patient.Email,
            dateOfBirth = patient.DateOfBirth,
            phoneNumber = patient.PhoneNumber,
            age = patient.DateOfBirth.HasValue ? (int?)((DateTime.UtcNow - patient.DateOfBirth.Value).TotalDays / 365.2425) : (int?)null,
            diagnosis = ended.DiagnosisTextSnapshot,
            diagnosisFileUrl = ended.DiagnosisFileUrlSnapshot,
            profilePicture = patient.ProfilePicture,
            createdAt = patient.CreatedAt,
            isFormer = true,
            leftAt = ended.EndedAt
        };
    }

    return new
    {
        patient.Id,
        patient.Name,
        patient.Email,
        dateOfBirth = patient.DateOfBirth,
        phoneNumber = patient.PhoneNumber,
        age = patient.DateOfBirth.HasValue ? (int?)((DateTime.UtcNow - patient.DateOfBirth.Value).TotalDays / 365.25) : (int?)null,
        diagnosis = patient.DiagnosisText,
        diagnosisFileUrl = patient.DiagnosisFileUrl,
        profilePicture = patient.ProfilePicture,
        createdAt = patient.CreatedAt,
        isFormer = false
    };
}

public async Task UpdatePatientDiagnosisAsync(int doctorId, int patientId, string diagnosis, string? diagnosisFileUrl = null)
{
    var doctor = await _doctorRepo.GetByIdAsync(doctorId);
    if (doctor == null)
        throw new Exception("Doctor not found");

    var patient = await _patientRepo.GetByIdAsync(patientId);
    if (patient == null)
        throw new Exception("Patient not found");

    var active = await _relationshipRepo.GetActiveAsync(doctorId, patientId);
    if (active == null)
        throw new Exception("Patient is no longer assigned to you. Plans and reports are read-only.");

    patient.DiagnosisText = diagnosis;
    if (!string.IsNullOrEmpty(diagnosisFileUrl))
    {
        patient.DiagnosisFileUrl = diagnosisFileUrl;
    }
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
        profilePicture = doctor.ProfilePicture,
        cv = doctor.CvFileUrl,
        phoneNumber = doctor.PhoneNumber,
        dateOfBirth = doctor.DateOfBirth,
        age = doctor.DateOfBirth.HasValue ? (int?)((DateTime.UtcNow - doctor.DateOfBirth.Value).TotalDays / 365.2425) : (int?)null,
        communicationInfo = doctor.CommunicationInfo,
        address = doctor.Address,
        cvText = doctor.CvText,
        createdAt = doctor.CreatedAt,
        averageRating = doctor.AverageRating
    };
}

public async Task UpdateDoctorProfileAsync(int doctorId, string? profilePicture, string? cv,
    string? name = null, string? phoneNumber = null, string? communicationInfo = null, string? address = null, DateTime? dateOfBirth = null, string? cvText = null)
{
    var doctor = await _doctorRepo.GetByIdAsync(doctorId);
    if (doctor == null)
        throw new Exception("Doctor not found");

    if (profilePicture != null)
        doctor.ProfilePicture = profilePicture;

    if (cv != null)
        doctor.CvFileUrl = cv;

    if (name != null)
        doctor.Name = name;

    if (phoneNumber != null)
        doctor.PhoneNumber = phoneNumber;

    if (communicationInfo != null)
        doctor.CommunicationInfo = communicationInfo;

    if (address != null)
        doctor.Address = address;

    if (dateOfBirth.HasValue)
        doctor.DateOfBirth = dateOfBirth.Value;

    if (cvText != null)
        doctor.CvText = cvText;

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
        profilePicture = d.ProfilePicture,
        cv = d.CvFileUrl,
        phoneNumber = d.PhoneNumber,
        address = d.Address,
        communicationInfo = d.CommunicationInfo,
        cvText = d.CvText,
        dateOfBirth = d.DateOfBirth,
        age = d.DateOfBirth.HasValue ? (int?)((DateTime.UtcNow - d.DateOfBirth.Value).TotalDays / 365.2425) : (int?)null,
        averageRating = d.AverageRating,
        createdAt = d.CreatedAt,
        patients = d.Patients?.Select(p => new
        {
            p.Id,
            p.Name,
            p.Email,
            dateOfBirth = p.DateOfBirth,
            phoneNumber = p.PhoneNumber,
            age = p.DateOfBirth.HasValue ? (int?)((DateTime.UtcNow - p.DateOfBirth.Value).TotalDays / 365.2425) : (int?)null,
            profilePicture = p.ProfilePicture
        }) ?? Enumerable.Empty<object>(),
        weeklyReports = d.WeeklyReports?.Select(r => new
        {
            r.Id,
            patientId = r.PatientId,
            patientName = r.Patient?.Name,
            startDate = r.StartDate,
            endDate = r.EndDate,
            totalHours = r.TotalHours,
            doctorNotes = r.DoctorNotes,
            aiSummary = r.AiSummary
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
        profilePicture = doctor.ProfilePicture,
        cv = doctor.CvFileUrl,
        phoneNumber = doctor.PhoneNumber,
        address = doctor.Address,
        communicationInfo = doctor.CommunicationInfo,
        cvText = doctor.CvText,
        dateOfBirth = doctor.DateOfBirth,
        age = doctor.DateOfBirth.HasValue ? (int?)((DateTime.UtcNow - doctor.DateOfBirth.Value).TotalDays / 365.2425) : (int?)null,
        averageRating = doctor.AverageRating,
        createdAt = doctor.CreatedAt,
        patients = doctor.Patients?.Select(p => new
        {
            p.Id,
            p.Name,
            p.Email,
            dateOfBirth = p.DateOfBirth,
            phoneNumber = p.PhoneNumber,
            age = p.DateOfBirth.HasValue ? (int?)((DateTime.UtcNow - p.DateOfBirth.Value).TotalDays / 365.2425) : (int?)null,
            profilePicture = p.ProfilePicture
        }) ?? Enumerable.Empty<object>(),
        weeklyReports = doctor.WeeklyReports?.Select(r => new
        {
            r.Id,
            patientId = r.PatientId,
            patientName = r.Patient?.Name,
            startDate = r.StartDate,
            endDate = r.EndDate,
            totalHours = r.TotalHours,
            doctorNotes = r.DoctorNotes,
            aiSummary = r.AiSummary
        }) ?? Enumerable.Empty<object>()
    };
}

    }
}
