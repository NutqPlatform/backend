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

        public PatientService(IPatientRepository patientRepo, IDoctorRepository doctorRepo, IWeeklyReportRepository weeklyReportRepo)
        {
            _patientRepo = patientRepo;
            _doctorRepo = doctorRepo;
            _weeklyReportRepo = weeklyReportRepo;
        }

        public async Task<object> GetPatientProfileAsync(int patientId)
        {
            var patient = await _patientRepo.GetByIdAsync(patientId);
            if (patient == null)
                throw new Exception("Patient not found");

            return new
            {
                patient.Id,
                patient.Name,
                patient.Email,
                patient.Age,
                patient.Diagnosis,
                patient.ProfilePicture,
                DoctorId = patient.DoctorId
            };
        }

        public async Task UpdatePatientProfileAsync(int patientId, string? profilePicture)
        {
            var patient = await _patientRepo.GetByIdAsync(patientId);
            if (patient == null)
                throw new Exception("Patient not found");

            if (profilePicture != null)
                patient.ProfilePicture = profilePicture;

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

            var doctor = await _doctorRepo.GetByIdAsync(patient.DoctorId);
            if (doctor == null)
                return null;

            var patientsWithSameDoctor = await _patientRepo.GetByDoctorIdAsync(doctor.Id);
            var weeklyReports = await _weeklyReportRepo.GetByPatientIdAsync(patientId);

            return new
            {
                doctor.Id,
                doctor.Name,
                doctor.Email,
                doctor.ProfilePicture,
                doctor.CV,
                Patients = patientsWithSameDoctor?.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Age,
                    p.ProfilePicture
                }) ?? Enumerable.Empty<object>(),
                Communication = weeklyReports?.Select(r => new
                {
                    r.Id,
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
