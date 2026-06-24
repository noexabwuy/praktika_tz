using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using api.Data;
using api.Models.DTOs;
using Microsoft.Extensions.Logging;

namespace api.Controllers
{
    /// <summary>
    /// Контроллер для управления пользователями (доступен только для Manager, Admin, Director)
    /// </summary>
    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = "Manager,Admin,Director")]  // Только эти роли имеют доступ
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ApplicationDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Получение списка пользователей с возможностью фильтрации по роли
        /// </summary>
        /// <param name="role">Фильтр по роли пользователя (например: Director, Admin, Manager, Applicant)</param>
        /// <returns>Список пользователей в формате UserResponseDto</returns>
        /// <remarks>
        /// Права доступа:
        /// - Получать списки пользователей могут только Manager, Admin и Director
        ///
        /// Примеры запросов:
        /// - GET /api/users — получить всех пользователей
        /// - GET /api/users?role=Director — получить всех директоров
        /// - GET /api/users?role=Admin — получить всех администраторов
        /// - GET /api/users?role=Manager — получить всех менеджеров
        /// - GET /api/users?role=Applicant — получить всех заявителей
        /// </remarks>
        /// <response code="200">Успешное получение списка пользователей</response>
        /// <response code="400">Передана недопустимая роль</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="403">Недостаточно прав (требуется роль Manager, Admin или Director)</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<UserResponseDto>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetUsers([FromQuery] string? role = null)
        {

            _logger.LogInformation("Запрошен список пользователей. Фильтр по роли: {Role}", role ?? "Все");

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var allowedRoles = new[] { "Manager", "Admin", "Director" };

            if (string.IsNullOrEmpty(userRole) || !allowedRoles.Contains(userRole, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Доступ отклонен: Пользователь с ролью '{Role}' не имеет прав.", userRole ?? "Unknown");
                return Forbid();
            }

            // Базовый запрос
            var query = _context.Users.AsQueryable();

            // Фильтр по роли (если передан)
            if (!string.IsNullOrEmpty(role))
            {
                // Список допустимых ролей
                var validRoles = new[] { "Manager", "Admin", "Director", "Applicant" };

                // Проверка, что переданная роль допустима
                if (!validRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Попытка фильтрации по недопустимой роли: '{Role}'", role);

                    return BadRequest(new
                    {
                        message = $"Недопустимое значение роли '{role}'. Допустимые значения: {string.Join(", ", validRoles)}"
                    });
                }

                // Применяем фильтр (регистронезависимо)
                query = query.Where(u => u.Role.ToLower() == role.ToLower());
            }

            // Выполнение запроса и преобразование в DTO
            var users = await query
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Role = u.Role
                })
                .ToListAsync();

            _logger.LogDebug("Успешно получено пользователей: {Count}", users.Count);
            
            return Ok(users);
        }
    }
}