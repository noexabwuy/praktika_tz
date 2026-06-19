using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models.Entities
{
    public class StatusHistory
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid ApplicationId { get; set; }
        public Guid ChangedById { get; set; }
        
        [MaxLength(50)]
        public string OldStatus { get; set; } = "";
        
        [MaxLength(50)]
        public string NewStatus { get; set; } = "";
        
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        
        // Связи
        [ForeignKey("ApplicationId")]
        public Application Application { get; set; } = null!;
        
        [ForeignKey("ChangedById")]
        public User ChangedBy { get; set; } = null!;
    }
}