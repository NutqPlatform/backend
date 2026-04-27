namespace Nutq.Core.Auth.DTOs
{
    public class PatientRegisterDto
    {
        public string InvitationCode { get; set; } = string.Empty; // كود الدعوة من الدكتور
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
