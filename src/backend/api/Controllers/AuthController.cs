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
using Microsoft.Extensions.Logging;

namespace api.Controllers
{
    /// <summary>
    /// Контроллер для аутентификации и регистрации пользователей
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ApplicationDbContext context, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Регистрация нового пользователя
        /// </summary>
        /// <param name="dto">Данные для регистрации (логин, пароль, ФИО, email)</param>
        /// <returns>Информация о зарегистрированном пользователе</returns>
        /// <response code="200">Регистрация успешно завершена</response>
        /// <response code="400">Некорректные данные запроса (логин/пароль пустые, пароль слишком короткий/длинный, логин уже занят)</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            _logger.LogInformation("Попытка регистрации пользователя с логином: {Login}, Email: {Email}", dto.Login, dto.Email);

            if (string.IsNullOrWhiteSpace(dto.Login) || string.IsNullOrWhiteSpace(dto.Password))
            {
                _logger.LogWarning("Регистрация отклонена: передан пустой логин или пароль.");
                return BadRequest(new { message = "Логин и пароль обязательны." });
            }

            if (dto.Password.Length < 8)
            {
                _logger.LogWarning("Регистрация отклонена: пароль для логина {Login} слишком короткий.", dto.Login);
                return BadRequest(new { message = "Пароль должен содержать минимум 8 символов." });
            }

            if (dto.Password.Length > 72)
            {
                _logger.LogWarning("Регистрация отклонена: пароль для логина {Login} слишком длинный.", dto.Login);
                return BadRequest(new { message = "Пароль не должен превышать 72 символа." });
            }

            var exists = await _context.Users.AnyAsync(u => u.Login.ToLower() == dto.Login.ToLower());
            if (exists)
            {
                _logger.LogWarning("Регистрация отклонена: логин '{Login}' уже занят.", dto.Login);
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

            _logger.LogInformation("Пользователь {Login} успешно зарегистрирован с ID: {UserId}.", userDto.Login, userDto.Id);

            return Ok(new { message = "Регистрация успешна!", user = userDto });
        }

        /// <summary>
        /// Вход пользователя в систему
        /// </summary>
        /// <param name="dto">Данные для входа (логин и пароль)</param>
        /// <returns>JWT-токен и информация о пользователе</returns>
        /// <response code="200">Успешный вход, возвращён JWT-токен</response>
        /// <response code="401">Неверный логин или пароль</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            _logger.LogInformation("Запрос на вход в систему для логина: {Login}", dto.Login);
            
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login.ToLower() == dto.Login.ToLower());

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Неудачная попытка входа: неверный пароль для логина '{Login}'", dto.Login);
                return Unauthorized(new { message = "Неверный логин или пароль." });
            }

            string token = GenerateJwtToken(user);

            _logger.LogInformation("Пользователь {Login} (Роль: {Role}) успешно авторизован.", user.Login, user.Role);

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

        /// <summary>
        /// Генерация JWT-токена для пользователя
        /// </summary>
        /// <param name="user">Пользователь, для которого генерируется токен</param>
        /// <returns>Строка JWT-токена</returns>
        private string GenerateJwtToken(User user)
        {
            _logger.LogDebug("Начало генерации JWT-токена для UserId: {UserId}", user.Id);

            var secretKey = Environment.GetEnvironmentVariable("JwtSettings__Secret") ?? _configuration["JwtSettings:Secret"];
            var issuer = Environment.GetEnvironmentVariable("JwtSettings__Issuer") ?? _configuration["JwtSettings:Issuer"];
            var audience = Environment.GetEnvironmentVariable("JwtSettings__Audience") ?? _configuration["JwtSettings:Audience"];
            
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
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            _logger.LogDebug("JWT-токен успешно сформирован для подписи.");

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}