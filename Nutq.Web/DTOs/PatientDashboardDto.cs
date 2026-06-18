namespace Nutq.Web.DTOs.PatientDashboard
{
    public class PatientDashboardDto
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public string? PlanStatus { get; set; }
        public bool IsArchived { get; set; }
        public double ProgressPercentage { get; set; }
        public List<ExerciseProgressDto> Exercises { get; set; } = new();
    }
}
