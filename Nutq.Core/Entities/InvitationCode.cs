using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nutq.Core.Entities
{
    public class InvitationCode
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Doctor))]
        public int DoctorId { get; set; }

        [Required, MaxLength(50)]
        public string Code { get; set; } = null!;

        [Required, MaxLength(50)]
        public string Type { get; set; } = null!;

        [Required]
        public DateTime ExpireAt { get; set; }

        public bool Used { get; set; }

        // Navigation
        public Doctor Doctor { get; set; } = null!;
    }
}
