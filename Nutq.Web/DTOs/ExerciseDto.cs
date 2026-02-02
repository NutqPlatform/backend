namespace Nutq.Web.DTOs
{
    public class ExerciseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? Difficulty { get; set; }
        public int DifficultyId { get; set; }
        public string? ImageUrl { get; set; }
        public string? AssetUrl { get; set; }
    }
}
