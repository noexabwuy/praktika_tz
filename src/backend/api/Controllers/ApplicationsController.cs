using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Models.DTOs;
using api.Models.Entities;

namespace api.Controllers
{
    [ApiController]
    [Route("api/applications")]
    [Authorize]
    public class ApplicationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApplicationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ApplicationResponseDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetApplications(
            [FromQuery] bool my = false,
            [FromQuery] string? status = null,
            [FromQuery] Guid? directionId = null,
            [FromQuery] Guid? formatId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            // Получаем ID текущего пользователя из токена
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid currentUserId))
            {
                return Unauthorized(new { message = "Не удалось определить пользователя из токена." });
            }

            // Базовый запрос с подгрузкой связанных данных
            var query = _context.Applications
                .Include(a => a.Direction)
                .Include(a => a.Format)
                .Include(a => a.Author)
                .Include(a => a.AssignedTo)
                .AsQueryable();

            // Фильтр: только свои заявки (где AuthorId = текущий пользователь)
            if (my)
            {
                query = query.Where(a => a.AuthorId == currentUserId);
            }

            // Фильтр по статусу
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.Status == status);
            }

            // Фильтр по направлению
            if (directionId.HasValue)
            {
                query = query.Where(a => a.DirectionId == directionId.Value);
            }

            // Фильтр по формату
            if (formatId.HasValue)
            {
                query = query.Where(a => a.FormatId == formatId.Value);
            }

            // Фильтр по дате создания (от)
            if (fromDate.HasValue)
            {
                query = query.Where(a => a.CreatedAt >= fromDate.Value);
            }

            // Фильтр по дате создания (до)
            if (toDate.HasValue)
            {
                var endDate = toDate.Value.AddDays(1);
                query = query.Where(a => a.CreatedAt < endDate);
            }

            // Сортировка: сначала новые
            query = query.OrderByDescending(a => a.CreatedAt);

            // Преобразуем в DTO
            var applications = await query
                .Select(a => new ApplicationResponseDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    Status = a.Status,
                    DirectionId = a.DirectionId,
                    FormatId = a.FormatId,
                    AuthorId = a.AuthorId,
                    AssignedToId = a.AssignedToId,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt,
                    DirectionName = a.Direction != null ? a.Direction.Name : "",
                    FormatName = a.Format != null ? a.Format.Name : "",
                    AuthorName = a.Author != null ? a.Author.FullName : "",
                    AssignedToName = a.AssignedTo != null ? a.AssignedTo.FullName : null
                })
                .ToListAsync();

            return Ok(applications);
        }

        /// <summary>
        /// Создание новой учебной заявки
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApplicationResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Create([FromBody] CreateApplicationRequestDto dto)
        {
            // Получаем ID текущего пользователя из токена
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid currentUserId))
            {
                return Unauthorized(new { message = "Не удалось определить пользователя из токена." });
            }

            // Проверка на пустой запрос
            if (dto == null)
            {
                return BadRequest(new { message = "Некорректные данные запроса" });
            }

            // Проверка существования направления
            var directionExists = await _context.Directions.AnyAsync(d => d.Id == dto.DirectionId);
            if (!directionExists)
            {
                return BadRequest(new { message = "Указанное направление обучения не найдено." });
            }

            // Проверка существования формата
            var formatExists = await _context.TrainingFormats.AnyAsync(f => f.Id == dto.FormatId);
            if (!formatExists)
            {
                return BadRequest(new { message = "Указанный формат обучения не найден." });
            }

            // Создание новой заявки
            var newApplication = new Application
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                Status = "New",
                DirectionId = dto.DirectionId,
                FormatId = dto.FormatId,
                AuthorId = currentUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Applications.Add(newApplication);
            await _context.SaveChangesAsync();

            // Загружаем связанные данные для ответа
            var created = await _context.Applications
                .Include(a => a.Direction)
                .Include(a => a.Format)
                .Include(a => a.Author)
                .Include(a => a.AssignedTo)
                .FirstOrDefaultAsync(a => a.Id == newApplication.Id);

            // Проверка на null (на случай, если заявка не найдена)
            if (created == null)
            {
                return StatusCode(500, new { message = "Ошибка при создании заявки." });
            }

            var responseDto = new ApplicationResponseDto
            {
                Id = created.Id,
                Title = created.Title,
                Description = created.Description,
                Status = created.Status,
                DirectionId = created.DirectionId,
                FormatId = created.FormatId,
                AuthorId = created.AuthorId,
                AssignedToId = created.AssignedToId,
                CreatedAt = created.CreatedAt,
                UpdatedAt = created.UpdatedAt,
                DirectionName = created.Direction?.Name ?? "",
                FormatName = created.Format?.Name ?? "",
                AuthorName = created.Author?.FullName ?? "",
                AssignedToName = created.AssignedTo?.FullName
            };

            return CreatedAtAction(nameof(Create), new { id = responseDto.Id }, responseDto);
        }
    }
}