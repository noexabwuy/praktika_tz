using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models.Entities
{
    public class Comment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public Guid ApplicationId { get; set; }
        public Guid AuthorId { get; set; }
        
        [Required]
        public string Text { get; set; } = "";
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // связи
        [ForeignKey("ApplicationId")]
        public Application Application { get; set; } = null!;
        
        [ForeignKey("AuthorId")]
        public User Author { get; set; } = null!;
    }
}