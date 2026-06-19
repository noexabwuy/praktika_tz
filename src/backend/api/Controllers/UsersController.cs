using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Models.DTOs;

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

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
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

            return Ok(users);
        }
    }
}