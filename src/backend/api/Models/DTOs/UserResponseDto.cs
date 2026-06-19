namespace api.Models.DTOs
{
    public class UserResponseDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = "";
        public string Role { get; set; } = "";
    }
}