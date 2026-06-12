namespace api.Models.Entities
{
    public class AppUser
    {
        public Guid Id { get; set; } 
        public string FullName { get; set; } = "";
        public string Login { get; set; } = ""; 
        public string Email { get; set; } = ""; 
        public string PasswordHash { get; set; } = ""; 
        public string Role { get; set; } = "Applicant"; // Applicant, Manager, Admin, Director
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}