using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nutq.Core.Services
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepo;
        private readonly IDoctorRepository _doctorRepo;
        private readonly IWeeklyReportRepository _weeklyReportRepo;
        private readonly IDoctorPatientRelationshipRepository _relationshipRepo;

        public PatientService(
            IPatientRepository patientRepo,
            IDoctorRepository doctorRepo,
            IWeeklyReportRepository weeklyReportRepo,
            IDoctorPatientRelationshipRepository relationshipRepo)
        {
            _patientRepo = patientRepo;
            _doctorRepo = doctorRepo;
            _weeklyReportRepo = weeklyReportRepo;
            _relationshipRepo = relationshipRepo;
        }

        public async Task<object> GetPatientProfileAsync(int patientId)
        {
            var patient = await _patientRepo.GetByIdAsync(patientId);
            if (patient == null)
                throw new Exception("Patient not found");

            var lastEnded = await _relationshipRepo.GetLastEndedForPatientAsync(patientId);

            return new
            {
                patient.Id,
                patient.Name,
                patient.Email,
                dateOfBirth = patient.DateOfBirth,
                phoneNumber = patient.PhoneNumber,
                age = patient.DateOfBirth.HasValue ?
                    (int)((DateTime.UtcNow - patient.DateOfBirth.Value).TotalDays / 365.2425) : (int?)null,
                createdAt = patient.CreatedAt,
                diagnosis = patient.DiagnosisText,
                diagnosisFileUrl = patient.DiagnosisFileUrl,
                profilePicture = patient.ProfilePicture,
                doctorId = patient.DoctorId,
                hasDoctor = patient.DoctorId.HasValue,
                formerDoctorId = lastEnded?.DoctorId
            };
        }

        public async Task UpdatePatientProfileAsync(int patientId, string? profilePicture, string? phoneNumber = null, DateTime? dateOfBirth = null)
        {
            var patient = await _patientRepo.GetByIdAsync(patientId);
            if (patient == null)
                throw new Exception("Patient not found");

            if (profilePicture != null)
                patient.ProfilePicture = profilePicture;

            if (phoneNumber != null)
                patient.PhoneNumber = phoneNumber;

            if (dateOfBirth.HasValue)
                patient.DateOfBirth = dateOfBirth.Value;

            await _patientRepo.UpdateAsync(patient);
        }

        public async Task UpdatePatientPasswordAsync(int patientId, string currentPassword, string newPassword)
        {
            var patient = await _patientRepo.GetByIdAsync(patientId);
            if (patient == null)
                throw new Exception("Patient not found");

            if (patient.Password != currentPassword)
                throw new Exception("Current password is incorrect");

            patient.Password = newPassword;
            await _patientRepo.UpdateAsync(patient);
        }

        public async Task<object?> GetAttendingDoctorAsync(int patientId)
        {
            var patient = await _patientRepo.GetByIdAsync(patientId);
            if (patient == null)
                return null;

            if (!patient.DoctorId.HasValue)
                return null;

            var doctor = await _doctorRepo.GetByIdAsync(patient.DoctorId.Value);
            if (doctor == null)
                return null;

            var patientsWithSameDoctor = await _patientRepo.GetByDoctorIdAsync(doctor.Id);
            var weeklyReports = await _weeklyReportRepo.GetByPatientIdAsync(patientId);

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
                patients = patientsWithSameDoctor?.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Email,
                    dateOfBirth = p.DateOfBirth,
                    phoneNumber = p.PhoneNumber,
                    age = p.DateOfBirth.HasValue ? (int?)((DateTime.UtcNow - p.DateOfBirth.Value).TotalDays / 365.2425) : (int?)null,
                    createdAt = p.CreatedAt,
                    profilePicture = p.ProfilePicture
                }) ?? Enumerable.Empty<object>(),
                weeklyReports = weeklyReports?.Select(r => new
                {
                    r.Id,
                    patientId = r.PatientId,
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
