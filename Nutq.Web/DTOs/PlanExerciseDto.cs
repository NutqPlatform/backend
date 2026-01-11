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

        // بيانات التمرين المرتبط
        public ExerciseDto? Exercise { get; set; }
    }

}
