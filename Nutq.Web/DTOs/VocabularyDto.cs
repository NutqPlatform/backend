namespace Nutq.Web.DTOs
{
    public class VocabularyDto
    {
        public int Id { get; set; }
        public string WordArabic { get; set; } = string.Empty;
        public string WordEnglish { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? DifficultyLevelName { get; set; }
        public string? ImageUrl { get; set; }
        public string? SoundUrl { get; set; }
        public string? VideoUrl { get; set; }
    }
}
