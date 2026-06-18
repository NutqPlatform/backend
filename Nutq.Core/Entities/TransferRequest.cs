using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nutq.Core.Entities
{
    public class TransferRequest
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Patient))]
        public int PatientId { get; set; }

        [ForeignKey(nameof(FromDoctor))]
        public int? FromDoctorId { get; set; }

        [ForeignKey(nameof(ToDoctor))]
        public int ToDoctorId { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Pending";

        public string? Message { get; set; }

        public int? InitiatedByDoctorId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? RespondedAt { get; set; }

        public Patient Patient { get; set; } = null!;
        public Doctor? FromDoctor { get; set; }
        public Doctor ToDoctor { get; set; } = null!;
    }
}
