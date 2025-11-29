// Nutq.Web/DTOs/PlanExerciseDto.cs
namespace Nutq.Web.DTOs
{
    public class PlanExerciseDto
    {
        public int Id { get; set; }
        public int TherapyPlanId { get; set; }
        public int ExerciseId { get; set; }
        public int DurationMinutes { get; set; }
        public int Repetition { get; set; }
        public string? AiConstraints { get; set; }

        // بيانات التمرين المرتبط (اختياري حسب الحاجة)
        public ExerciseDto? Exercise { get; set; }
    }

    public class ExerciseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? Difficulty { get; set; }
    }
}
