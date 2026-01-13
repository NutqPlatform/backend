namespace Nutq.Web.DTOs
{
    public class DoctorAnalyticsDto
    {
        public int TotalPatients { get; set; }
        public int TotalPlans { get; set; }
        public int TotalExercises { get; set; }
        public double AverageCompletionRate { get; set; }
    }
}
