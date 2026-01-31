
namespace Nutq.Web.DTOs
{
    public class ExerciseProgressDto
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int PlanExerciseId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double? Score { get; set; }
        public bool Completed { get; set; }
        public string ExerciseName { get; set; } = null!;
    }
}
