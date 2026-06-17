namespace Nutq.Web.DTOs
{
    public class UpdateDiagnosisDto
    {
        public string? Diagnosis { get; set; }
        // Optional base64-encoded file content (data without data: prefix)
        public string? DiagnosisFileBase64 { get; set; }
        // Optional original filename (to preserve extension)
        public string? DiagnosisFileName { get; set; }
    }
}
