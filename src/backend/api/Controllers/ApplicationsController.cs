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

        /// <summary>
        /// Создание новой учебной заявки
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApplicationResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Create([FromBody] CreateApplicationRequestDto dto)
        {

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid currentUserId))
            {
                return Unauthorized(new { message = "Не удалось определить пользователя из токена." });
            }


            var directionExists = await _context.Directions.AnyAsync(d => d.Id == dto.DirectionId);
            if (!directionExists)
            {
                return BadRequest(new { message = "Указанное направление обучения не найдено." });
            }

            var formatExists = await _context.TrainingFormats.AnyAsync(f => f.Id == dto.TrainingFormatId);
            if (!formatExists)
            {
                return BadRequest(new { message = "Указанный формат обучения не найден." });
            }


            var newApplication = new Application
            {
                Id = Guid.NewGuid(),
                Title = $"Заявка на обучение от {DateTime.UtcNow:dd.MM.yyyy}", 
                Description = dto.Description,
                Status = "New",
                DirectionId = dto.DirectionId!.Value,
                FormatId = dto.TrainingFormatId!.Value,
                AuthorId = currentUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Applications.Add(newApplication);
            await _context.SaveChangesAsync();

            var responseDto = new ApplicationResponseDto
            {
                Id = newApplication.Id,
                DirectionId = newApplication.DirectionId,
                TrainingFormatId = newApplication.FormatId,
                Status = newApplication.Status,
                CreatedAt = newApplication.CreatedAt
            };

            return CreatedAtAction(nameof(Create), new { id = responseDto.Id }, responseDto);
        }

        /// <summary>
        /// Назначить ответственного менеджера на заявку
        /// Доступно только для: Manager
        /// </summary>
        [HttpPatch("{id}/assign")]
        [Authorize(Roles = "Manager")]
        [ProducesResponseType(typeof(ApplicationResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AssignApplication(Guid id, [FromBody] AssignApplicationRequestDto dto)
        {
            // 1. Находим заявку
            var application = await _context.Applications.FindAsync(id);
            if (application == null)
            {
                return NotFound(new { message = "Заявка не найдена." });
            }

            // 2. Находим пользователя, которого хотим назначить ответственным
            var employee = await _context.Users.FindAsync(dto.AssignedToId);
            if (employee == null)
            {
                return BadRequest(new { message = "Указанный пользователь не найден в системе." });
            }

            // Проверяем роль напрямую по строке из базы данных
            if (string.IsNullOrEmpty(employee.Role) || !employee.Role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Ответственным за выполнение заявки можно назначить ТОЛЬКО сотрудника с ролью Manager." });
            }

            string oldStatus = application.Status;

            // Если заявка была "Новая", при взятии в работу/назначении переводим её в "В работе"
            if (application.Status == "New")
            {
                application.Status = "InProgress";
            }

            // Получаем ID текущего авторизованного менеджера (кто совершает действие)
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            Guid.TryParse(currentUserIdClaim?.Value, out Guid currentUserId);

            // 3. Фиксируем изменение статуса в таблице StatusHistory
            if (oldStatus != application.Status)
            {
                var history = new StatusHistory
                {
                    Id = Guid.NewGuid(),
                    ApplicationId = application.Id,
                    OldStatus = oldStatus,
                    NewStatus = application.Status,
                    ChangedById = currentUserId,
                    ChangedAt = DateTime.UtcNow
                };
                _context.StatusHistories.Add(history);
            }

            // 4. Обновляем данные в заявке
            application.AssignedToId = dto.AssignedToId;
            application.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var responseDto = new ApplicationResponseDto
            {
                Id = application.Id,
                DirectionId = application.DirectionId,
                TrainingFormatId = application.FormatId,
                Status = application.Status,
                CreatedAt = application.CreatedAt
            };

            return Ok(responseDto);
        }

        /// <summary>
        /// Изменить статус заявки менеджером
        /// Доступно только для: Manager
        /// </summary>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Manager")]
        [ProducesResponseType(typeof(ApplicationResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateApplicationStatusRequestDto dto)
        {
            // 1. Находим заявку
            var application = await _context.Applications.FindAsync(id);
            if (application == null)
            {
                return NotFound(new { message = "Заявка не найдена." });
            }

            if (application.Status == dto.Status)
            {
                return BadRequest(new { message = $"Заявка уже находится в статусе {dto.Status}." });
            }

            // Валидация логики ТЗ: нельзя согласовать или отклонить, пока не назначен менеджер
            if ((dto.Status == "Approved" || dto.Status == "Rejected") && application.AssignedToId == null)
            {
                return BadRequest(new { message = "Нельзя перевести заявку в финальный статус без назначенного ответственного менеджера." });
            }

            // Получаем ID текущего менеджера для логов
            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            Guid.TryParse(currentUserIdClaim?.Value, out Guid currentUserId);

            // 2. Добавляем запись в историю статусов
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

            // 3. Переключаем статус
            application.Status = dto.Status;
            application.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var responseDto = new ApplicationResponseDto
            {
                Id = application.Id,
                DirectionId = application.DirectionId,
                TrainingFormatId = application.FormatId,
                Status = application.Status,
                CreatedAt = application.CreatedAt
            };

            return Ok(responseDto);
        }
    }
}