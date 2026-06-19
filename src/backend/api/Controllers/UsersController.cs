using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Models.DTOs;
using api.Models.Enums;

namespace api.Controllers
{
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

        [HttpGet]
        [ProducesResponseType(typeof(List<UserResponseDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]

        // GET /api/users - вызов всех
        // GET /api/users?role=Director - вызов всех директоров
        // GET /api/users?role=Admin - вызов всех администраторов
        // GET /api/users?role=Manager - вызов всех менеджеров
        // GET /api/users?role=Applicant - вызов всех заявителей
        public async Task<IActionResult> GetUsers([FromQuery] UserRole? role = null)
        {
            // Базовый запрос
            var query = _context.Users.AsQueryable();

            if (role.HasValue)
            {
                var roleString = role.Value.ToString();
                query = query.Where(u => u.Role == roleString);
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