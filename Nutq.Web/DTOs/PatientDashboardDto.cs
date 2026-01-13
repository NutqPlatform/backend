namespace Nutq.Web.DTOs.PatientDashboard
{
    public class PatientDashboardDto
    {
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public List<ExerciseProgressDto> Exercises { get; set; } = new();
    }
}
