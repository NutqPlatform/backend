using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nutq.Core.Entities
{
    public class InvitationCode
{
    [Key]
    public int Id { get; set; }

    [MaxLength(50)]
    public string Code { get; set; } = null!;

    // doctor / patient
    [Required, MaxLength(20)]
    public string Type { get; set; } = null!;

    // لو Patient → لازم يكون ليه DoctorId
    // لو Doctor → تبقى null
    public int? DoctorId { get; set; }
    public Doctor? Doctor { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool Used { get; set; } = false;
}

}
