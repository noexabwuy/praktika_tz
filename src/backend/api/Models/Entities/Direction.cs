using System.ComponentModel.DataAnnotations;

namespace api.Models.Entities
{
    public class Direction
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = "";

        // Связи
        public ICollection<Application> Applications { get; set; } = new List<Application>();
    }
}