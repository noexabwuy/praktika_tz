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

        /// <summary>
        /// PATCH /api/applications/{id}/assign
        /// Назначение ответственного менеджера на заявку
        /// </summary>
        [Authorize(Roles = "Manager")]
        [HttpPatch("{id}/assign")]
        public async Task<IActionResult> AssignApplication(Guid id, [FromBody] AssignApplicationRequestDto dto)
        {
            var application = await _context.Applications
                .Include(a => a.Direction)
                .Include(a => a.Format)
                .Include(a => a.Author)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null)
            {
                return NotFound(new { message = "Заявка не найдена." });
            }

            var targetUser = await _context.Users.FindAsync(dto.AssignedToId);
            if (targetUser == null)
            {
                return BadRequest(new { message = "Указанный пользователь не найден." });
            }

            if (!string.Equals(targetUser.Role, "Manager", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Назначить ответственным можно только пользователя с ролью Manager." });
            }

            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            Guid.TryParse(currentUserIdClaim?.Value, out Guid currentUserId);

            if (string.Equals(application.Status, "New", StringComparison.OrdinalIgnoreCase))
            {
                var history = new StatusHistory
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = application.Id,
                    OldStatus = application.Status,
                    NewStatus = "InProgress",
                    ChangedById = currentUserId,
                    ChangedAt = DateTime.UtcNow
                };
                _context.StatusHistories.Add(history);
                application.Status = "InProgress";
            }

            application.AssignedToId = dto.AssignedToId;
            application.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _context.Entry(application).Reference(a => a.AssignedTo).LoadAsync();

            var responseDto = new ApplicationResponseDto
            {
                Id = application.Id,
                Title = application.Title,
                Description = application.Description,
                Status = application.Status,
                DirectionId = application.DirectionId,
                FormatId = application.FormatId,
                AuthorId = application.AuthorId,
                AssignedToId = application.AssignedToId,
                CreatedAt = application.CreatedAt,
                UpdatedAt = application.UpdatedAt,
                DirectionName = application.Direction?.Name ?? "",
                FormatName = application.Format?.Name ?? "",
                AuthorName = application.Author?.FullName ?? "",
                AssignedToName = application.AssignedTo?.FullName
            };

            return Ok(responseDto);
        }

        /// <summary>
        /// PATCH /api/applications/{id}/status
        /// Изменение статуса заявки менеджером
        /// </summary>
        [Authorize(Roles = "Manager")]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateApplicationStatusRequestDto dto)
        {
            var application = await _context.Applications
                .Include(a => a.Direction)
                .Include(a => a.Format)
                .Include(a => a.Author)
                .Include(a => a.AssignedTo)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null)
            {
                return NotFound(new { message = "Заявка не найдена." });
            }

            if ((string.Equals(dto.Status, "Approved", StringComparison.OrdinalIgnoreCase) || 
                 string.Equals(dto.Status, "Rejected", StringComparison.OrdinalIgnoreCase) || 
                 string.Equals(dto.Status, "Completed", StringComparison.OrdinalIgnoreCase)) && 
                application.AssignedToId == null)
            {
                return BadRequest(new { message = "Нельзя перевести заявку в финальный статус без назначенного ответственного менеджера." });
            }

            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            Guid.TryParse(currentUserIdClaim?.Value, out Guid currentUserId);

            var history = new StatusHistory
            {
                Id = Guid.NewGuid(),
                ApplicationId = application.Id,
                OldStatus = application.Status,
                NewStatus = dto.Status,
                ChangedById = currentUserId,
                ChangedAt = DateTime.UtcNow
            };
            _context.StatusHistories.Add(history);

            application.Status = dto.Status;
            application.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var responseDto = new ApplicationResponseDto
            {
                Id = application.Id,
                Title = application.Title,
                Description = application.Description,
                Status = application.Status,
                DirectionId = application.DirectionId,
                FormatId = application.FormatId,
                AuthorId = application.AuthorId,
                AssignedToId = application.AssignedToId,
                CreatedAt = application.CreatedAt,
                UpdatedAt = application.UpdatedAt,
                DirectionName = application.Direction?.Name ?? "",
                FormatName = application.Format?.Name ?? "",
                AuthorName = application.Author?.FullName ?? "",
                AssignedToName = application.AssignedTo?.FullName
            };

            return Ok(responseDto);
        }

        /// <summary>
        /// СИСТЕМА КОММЕНТАРИЕВ (POST /api/applications/{id}/comments)
        /// </summary>
        [HttpPost("{id}/comments")]
        [ProducesResponseType(typeof(CommentResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AddComment(Guid id, [FromBody] CreateCommentRequestDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid currentUserId))
            {
                return Unauthorized(new { message = "Не удалось определить пользователя из токена." });
            }

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var application = await _context.Applications.FindAsync(id);
            if (application == null)
            {
                return NotFound(new { message = "Заявка не найдена." });
            }

            // Проверка прав: заявитель может комментировать только свою заявку
            if (userRole == "Applicant" && application.AuthorId != currentUserId)
            {
                return Forbid();
            }

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                ApplicationId = id,
                AuthorId = currentUserId,
                Text = dto.Text,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Подгружаем имя автора для красивого ответа
            var authorName = await _context.Users
                .Where(u => u.Id == currentUserId)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync() ?? "";

            var responseDto = new CommentResponseDto
            {
                Id = comment.Id,
                ApplicationId = comment.ApplicationId,
                AuthorId = comment.AuthorId,
                AuthorName = authorName,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt
            };

            return Ok(responseDto);
        }

        /// <summary>
        /// ПОЛУЧЕНИЕ КОММЕНТАРИЕВ (GET /api/applications/{id}/comments)
        /// </summary>
        [HttpGet("{id}/comments")]
        [ProducesResponseType(typeof(List<CommentResponseDto>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetComments(Guid id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid currentUserId))
            {
                return Unauthorized(new { message = "Не удалось определить пользователя из токена." });
            }

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var application = await _context.Applications.FindAsync(id);
            if (application == null)
            {
                return NotFound(new { message = "Заявка не найдена." });
            }

            // Проверка прав: заявитель может видеть комментарии только к своей заявке
            if (userRole == "Applicant" && application.AuthorId != currentUserId)
            {
                return Forbid();
            }

            var comments = await _context.Comments
                .Where(c => c.ApplicationId == id)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new CommentResponseDto
                {
                    Id = c.Id,
                    ApplicationId = c.ApplicationId,
                    AuthorId = c.AuthorId,
                    AuthorName = c.Author.FullName,
                    Text = c.Text,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(comments);
        }
    }
}