namespace Nutq.Core.Commands
{
    public class AddPlanExerciseCommand
    {
        public int ExerciseId { get; set; }
        public int DurationMinutes { get; set; } = 10;
        public int Repetition { get; set; } = 1;
        public string? AiConstraints { get; set; }  
    }
}
