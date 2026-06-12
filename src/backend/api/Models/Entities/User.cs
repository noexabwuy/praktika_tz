using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace api.Models.Entities
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = "";

        [Required]
        [MaxLength(50)]
        public string Login { get; set; } = "";

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; } = "";

        [MaxLength(20)]
        public string Role { get; set; } = "Applicant";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        // Связи
        [InverseProperty("Author")]
        public ICollection<Application> AuthoredApplications { get; set; } = new List<Application>();
        [InverseProperty("AssignedTo")]
        public ICollection<Application> AssignedApplications { get; set; } = new List<Application>();
        [InverseProperty("Author")]
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        [InverseProperty("ChangedBy")]
        public ICollection<StatusHistory> StatusHistories { get; set; } = new List<StatusHistory>();
        [InverseProperty("User")]
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}