using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nutq.Core.Entities
{
    public class DoctorPatientRelationship
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Doctor))]
        public int DoctorId { get; set; }

        [ForeignKey(nameof(Patient))]
        public int PatientId { get; set; }

        public DateTime AssignedAt { get; set; }

        public DateTime? EndedAt { get; set; }

        public string? DiagnosisTextSnapshot { get; set; }

        public string? DiagnosisFileUrlSnapshot { get; set; }

        public Doctor Doctor { get; set; } = null!;
        public Patient Patient { get; set; } = null!;
    }
}
