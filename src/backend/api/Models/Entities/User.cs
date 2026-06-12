namespace api.Models.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FullName { get; set; } = "";
        public string Login { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string Role { get; set; } = "Applicant";  // Applicant, Manager, Admin, Director
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}