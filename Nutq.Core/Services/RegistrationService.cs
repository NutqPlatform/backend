using Nutq.Core.Commands;
using Nutq.Core.Entities;
using Nutq.Core.Interfaces;

namespace Nutq.Core.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly IDoctorRepository _doctorRepo;
        private readonly IPatientRepository _patientRepo;
        private readonly IInvitationCodeRepository _inviteRepo;
        private readonly IDoctorPatientRelationshipRepository _relationshipRepo;

        public RegistrationService(
            IDoctorRepository doctorRepo,
            IPatientRepository patientRepo,
            IInvitationCodeRepository inviteRepo,
            IDoctorPatientRelationshipRepository relationshipRepo)
        {
            _doctorRepo = doctorRepo;
            _patientRepo = patientRepo;
            _inviteRepo = inviteRepo;
            _relationshipRepo = relationshipRepo;
        }

        public async Task<bool> RegisterDoctorAsync(DoctorRegisterCommand command)
        {
            var code = await _inviteRepo.GetValidCodeAsync(command.InvitationCode, "Doctor");
            if (code == null)
                return false;

            var doctor = new Doctor
            {
                Name = command.Name,
                Email = command.Email,
                Password = command.Password
            };

            if (!string.IsNullOrWhiteSpace(command.PhoneNumber))
                doctor.PhoneNumber = command.PhoneNumber;

            doctor.CreatedAt = DateTime.UtcNow;
            await _doctorRepo.AddAsync(doctor);

          
            await _inviteRepo.MarkUsedAsync(code.Id);

            return true;
        }

    public async Task<bool> RegisterPatientAsync(PatientRegisterCommand command)
{
    var code = await _inviteRepo.GetValidCodeAsync(command.InvitationCode, "Patient");
    if (code == null || code.DoctorId == null)
        return false; 

    var patient = new Patient
    {
        DoctorId = code.DoctorId.Value, 
        Name = command.Name,
        Email = command.Email,
        Password = command.Password,
        DateOfBirth = command.DateOfBirth.HasValue
            ? DateTime.SpecifyKind(command.DateOfBirth.Value, DateTimeKind.Utc)
            : null,
            DiagnosisText = "",
            PhoneNumber = command.PhoneNumber ?? string.Empty,
            CreatedAt = DateTime.UtcNow
    };

    await _patientRepo.AddAsync(patient);

    await _relationshipRepo.AddAsync(new DoctorPatientRelationship
    {
        DoctorId = code.DoctorId.Value,
        PatientId = patient.Id,
        AssignedAt = DateTime.UtcNow
    });

    await _inviteRepo.MarkUsedAsync(code.Id);

    return true;
}
    }
}
