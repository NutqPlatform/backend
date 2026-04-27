using System;

namespace Nutq.Core.Commands
{
    public class ExerciseProgressCommand
    {
        public int PatientId { get; set; }
        public int PlanExerciseId { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public double? Score { get; set; }
        public bool Completed { get; set; }
    }
}
