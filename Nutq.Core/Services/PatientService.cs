using Nutq.Core.Entities;
using Nutq.Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace Nutq.Core.Services
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepo;
        private readonly IDoctorRepository _doctorRepo;

        public PatientService(IPatientRepository patientRepo, IDoctorRepository doctorRepo)
        {
            _patientRepo = patientRepo;
            _doctorRepo = doctorRepo;
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

            return new
            {
                doctor.Id,
                doctor.Name,
                doctor.Email,
                doctor.ProfilePicture,
                doctor.CV
            };
        }
    }
}
