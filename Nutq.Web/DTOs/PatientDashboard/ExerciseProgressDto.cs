namespace Nutq.Web.DTOs.PatientDashboard
{
    public class ExerciseProgressDto
    {
        public int PlanExerciseId { get; set; }
        public int ExerciseId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public double? Score { get; set; }
        public bool Started { get; set; }
        public int CurrentRepetition { get; set; } = 1;
        public int TotalRepetitions { get; set; } = 1;
    }
}
