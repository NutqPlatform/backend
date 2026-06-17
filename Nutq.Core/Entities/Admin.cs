using System.ComponentModel.DataAnnotations;

namespace Nutq.Core.Entities
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required, MaxLength(255)]
        public string Password { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<InvitationCode>? InvitationCodes { get; set; }
    }
}
