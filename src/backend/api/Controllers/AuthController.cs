using api.Data;
using api.Models.Entities;
using api.Models.DTOs;
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
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // 1. ЭНДПОИНТ РЕГИСТРАЦИИ
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Login) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest(new { message = "Логин и пароль обязательны." });
            }

            if (dto.Password.Length < 8)
            {
                return BadRequest(new { message = "Пароль должен содержать минимум 8 символов." });
            }

            if (dto.Password.Length > 72)
            {
                return BadRequest(new { message = "Пароль не должен превышать 72 символа." });
            }

            var exists = await _context.Users.AnyAsync(u => u.Login.ToLower() == dto.Login.ToLower());
            if (exists)
            {
                return BadRequest(new { message = "Этот логин уже занят." });
            }

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Login = dto.Login,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "Applicant", 
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            var userDto = new UserDto
            {
                Id = newUser.Id,
                FullName = newUser.FullName,
                Login = newUser.Login,
                Email = newUser.Email,
                Role = newUser.Role
            };

            return Ok(new { message = "Регистрация успешна!", user = userDto });
        }

        // 2. ЭНДПОИНТ ВХОДА
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login.ToLower() == dto.Login.ToLower());

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Неверный логин или пароль." });
            }

            string token = GenerateJwtToken(user);

            var userDto = new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Login = user.Login,
                Email = user.Email,
                Role = user.Role
            };

            return Ok(new AuthResponseDto
            {
                Token = token,
                User = userDto
            });
        }

        // Вспомогательный метод генерации JWT
        private string GenerateJwtToken(User user)
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