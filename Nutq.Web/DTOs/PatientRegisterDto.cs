namespace Nutq.Core.Auth.DTOs
{
    public class PatientRegisterDto
    {
        public string InvitationCode { get; set; } = string.Empty; // كود الدعوة من الدكتور
        public string Name { get; set; } = string.Empty;
        // ISO date for birth, optional
        public DateTime? DateOfBirth { get; set; }
        // optional phone number
        public string? PhoneNumber { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
