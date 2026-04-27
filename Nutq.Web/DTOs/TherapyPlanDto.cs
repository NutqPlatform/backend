using Nutq.Web.DTOs; 
public class TherapyPlanDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
public string Status { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? PatientId { get; set; }
    public string? PatientName { get; set; }
    public double? ProgressPercentage { get; set; }
    public List<PlanExerciseDto> Exercises { get; set; } = new List<PlanExerciseDto>();

}
