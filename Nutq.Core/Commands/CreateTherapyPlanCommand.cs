namespace Nutq.Core.Commands
{
    public class CreateTherapyPlanCommand
    {
        public string? Description { get; set; }
        public string? Status { get; set; } = "Active";
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime? EndDate { get; set; }
    }
}
