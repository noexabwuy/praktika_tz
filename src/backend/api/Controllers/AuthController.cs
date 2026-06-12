using api.Data;
using api.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public class RegisterDto
        {
            public string FullName { get; set; } = "";
            public string Login { get; set; } = "";
            public string Email { get; set; } = "";
            public string Password { get; set; } = "";
        }

        public class LoginDto
        {
            public string Login { get; set; } = "";
            public string Password { get; set; } = "";
        }

        // 1. ЭНДПОИНТ РЕГИСТРАЦИИ
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Login) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { message = "Логин и пароль обязательны." });
            }

            var exists = await _context.AppUsers.AnyAsync(u => u.Login.ToLower() == dto.Login.ToLower());
            if (exists)
            {
                return BadRequest(new { message = "Этот логин уже занят." });
            }

            var newAppUser = new AppUser
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Login = dto.Login,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "Applicant", 
                CreatedAt = DateTime.UtcNow
            };

            _context.AppUsers.Add(newAppUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Регистрация успешна!" });
        }

        // 2. ЭНДПОИНТ ВХОДА
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Login.ToLower() == dto.Login.ToLower());

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Неверный логин или пароль." });
            }

            string token = GenerateJwtToken(user);

            return Ok(new {
                message = "Успешный вход!",
                token = token,
                userId = user.Id,
                fullName = user.FullName,
                role = user.Role
            });
        }

        // Вспомогательный метод генерации JWT
        private string GenerateJwtToken(AppUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings.GetValue<string>("Secret");
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("FullName", user.FullName)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings.GetValue<string>("Issuer"),
                audience: jwtSettings.GetValue<string>("Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}