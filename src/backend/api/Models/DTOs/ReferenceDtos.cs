namespace api.Models.DTOs
{
    public class DirectionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class TrainingFormatDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = "";
        public string Login { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
    }
}
