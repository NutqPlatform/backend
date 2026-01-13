namespace Nutq.Web.DTOs.PatientDashboard
{
    public class ExerciseProgressDto
    {
        public int ExerciseId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public double? Score { get; set; }
    }
}
