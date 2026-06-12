using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models.Entities
{
    public class Application
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = "";

        public string Description { get; set; } = "";

        [MaxLength(50)]
        public string Status { get; set; } = "New";

        // Внешние ключи
        public Guid DirectionId { get; set; }
        public Guid FormatId { get; set; }
        public Guid AuthorId { get; set; }
        public Guid? AssignedToId { get; set; }

        // Даты
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Навигационные свойства (связи)
        [ForeignKey("DirectionId")]
        public Direction Direction { get; set; } = null!;

        [ForeignKey("FormatId")]
        public TrainingFormat Format { get; set; } = null!;

        [ForeignKey("AuthorId")]
        public User Author { get; set; } = null!;

        [ForeignKey("AssignedToId")]
        public User? AssignedTo { get; set; }

        // Коллекции для связанных данных
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<StatusHistory> StatusHistories { get; set; } = new List<StatusHistory>();
    }
}