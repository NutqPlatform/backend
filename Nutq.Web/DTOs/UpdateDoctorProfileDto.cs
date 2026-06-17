namespace Nutq.Web.DTOs
{
    public class UpdateDoctorProfileDto
    {
        public string? ProfilePicture { get; set; }
        public string? CV { get; set; }
        // Optional base64-encoded CV file
        public string? CvFileBase64 { get; set; }
        public string? CvFileName { get; set; }
        // Basic editable info
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CommunicationInfo { get; set; }
        public string? Address { get; set; }
        public string? CvText { get; set; }
        // Accept as string from client to avoid model-binding errors; controller will parse.
        public string? DateOfBirth { get; set; }
    }
}
