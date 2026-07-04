using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models.Entities
{
    public class AuditLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }

        [MaxLength(50)]
        public string Action { get; set; } = "";  // Create, Update, Delete

        [MaxLength(50)]
        public string Entity { get; set; } = "";  // Application, User, Direction и т.д.

        public Guid EntityId { get; set; }

        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

        // Связи
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}