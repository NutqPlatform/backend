namespace Nutq.Web.DTOs
{
    public class DoctorRegisterDto
    {
        public string InvitationCode { get; set; } = string.Empty; // الكود اللي أخده من الأدمن
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
