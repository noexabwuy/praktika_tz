namespace api.Models.DTOs
{
    public class LoginRequestDto
    {
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class RegisterRequestDto
    {
        public string FullName { get; set; } = "";
        public string Login { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = "";
        public UserDto User { get; set; } = null!;
    }
}