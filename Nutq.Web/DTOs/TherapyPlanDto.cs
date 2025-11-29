using Nutq.Web.DTOs; 
public class TherapyPlanDto
{
    public int Id { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<PlanExerciseDto> Exercises { get; set; } = new List<PlanExerciseDto>();

}
