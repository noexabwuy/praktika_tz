using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Models.DTOs;
using api.Models.Entities;
using Microsoft.Extensions.Logging;

namespace api.Controllers
{
    /// <summary>
    /// Контроллер для управления учебными заявками
    /// </summary>
    [ApiController]
    [Route("api/applications")]
    [Authorize]
    public class ApplicationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ApplicationsController> _logger;

        public ApplicationsController(ApplicationDbContext context, ILogger<ApplicationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Получение списка заявок с фильтрацией
        /// </summary>
        /// <param name="my">
        /// Фильтр по принадлежности заявки текущему пользователю:
        /// - true — только свои заявки (по умолчанию)
        /// - false — все заявки (только для ролей: Admin, Manager, Director)
        /// </param>
        /// <param name="status">Фильтр по статусу заявки (например: New, InProgress, NeedsInfo, Approved, Rejected, Completed)</param>
        /// <param name="directionId">Фильтр по идентификатору направления подготовки</param>
        /// <param name="formatId">Фильтр по идентификатору формата обучения</param>
        /// <param name="fromDate">Начальная дата создания заявки (включительно)</param>
        /// <param name="toDate">Конечная дата создания заявки (включительно)</param>
        /// <returns>Список заявок в формате ApplicationResponseDto</returns>
        /// <remarks>
        /// Права доступа:
        /// - Просмотр своих заявок (my = true) доступен всем авторизированным пользователям
        /// - Просмотр всех заявок (my = false) доступен только для ролей: Admin, Manager, Director
        /// </remarks>
        /// <response code="200">Успешное получение списка заявок</response>
        /// <response code="401">Пользователь не авторизован или не удалось определить UserId из токена</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<ApplicationResponseDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetApplications(
            [FromQuery] bool my = true,
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
                _logger.LogWarning("GetApplications: Не удалось определить пользователя из JWT токена.");
                return Unauthorized(new { message = "Не удалось определить пользователя из токена." });
            }

            // Получаем роль пользователя
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            _logger.LogInformation("Пользователь {UserId} (Роль: {Role}) запросил список заявок. Фильтр 'my': {My}", currentUserId, userRole, my);

            // Если my = false — проверяем, что пользователь имеет одну из разрешённых ролей
            if (!my)
            {
                var allowedRoles = new[] { "Admin", "Manager", "Director" };
                if (userRole == null || !allowedRoles.Contains(userRole))
                {
                    _logger.LogWarning("Доступ запрещен: Пользователь {UserId} с ролью Applicant пытался запросить ВСЕ заявки (my=false).", currentUserId);
                    return Forbid(); // 403 - не достаточно прав доступа
                }
            }

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
            
            _logger.LogDebug("Успешно возвращено {Count} заявок для пользователя {UserId}.", applications.Count, currentUserId);
            
            return Ok(applications);
        }

        /// <summary>
        /// Создание новой учебной заявки
        /// </summary>
        /// <param name="dto">Данные для создания заявки (название, описание, направление подготовки, формат обучения)</param>
        /// <returns>Созданная заявка в формате ApplicationResponseDto</returns>
        /// <remarks>
        /// Права доступа:
        /// - Доступно всем авторизированным пользователям
        /// </remarks>
        /// <response code="201">Заявка успешно создана</response>
        /// <response code="400">Некорректные данные запроса или указаны несуществующие направление/формат</response>
        /// <response code="401">Пользователь не авторизован или не удалось определить UserId из токена</response>
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
                _logger.LogWarning("Заявка отклонена: Отказ в доступе. Пользователь не авторизован.");
                return Unauthorized(new { message = "Не удалось определить пользователя из токена." });
            }

            _logger.LogInformation("Пользователь {UserId} инициировал создание заявки: '{Title}'", currentUserId, dto.Title);

            // Проверка на пустой запрос
            if (dto == null)
            {
                _logger.LogWarning("Заявка отклонена: Некорректные данные запроса.");
                return BadRequest(new { message = "Некорректные данные запроса" });
            }

            // Проверка существования направления
            var directionExists = await _context.Directions.AnyAsync(d => d.Id == dto.DirectionId);
            if (!directionExists)
            {
                _logger.LogWarning("Заявка отклонена: Направление с ID {DirectionId} не найдено.", dto.DirectionId);
                return BadRequest(new { message = "Указанное направление обучения не найдено." });
            }

            // Проверка существования формата
            var formatExists = await _context.TrainingFormats.AnyAsync(f => f.Id == dto.FormatId);
            if (!formatExists)
            {
                _logger.LogWarning("Заявка отклонена: Формат обучения с ID {FormatId} не найден.", dto.FormatId);
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

            _logger.LogInformation(
            "Заявка успешно создана. {@applicationcreation}",
            new { ApplicationId = responseDto.Id, AuthorId = currentUserId, Title = dto.Title }
            );

            return CreatedAtAction(nameof(Create), new { id = responseDto.Id }, responseDto);
        }

        /// <summary>
        /// Назначение ответственного менеджера на заявку
        /// </summary>
        /// <param name="id">Идентификатор заявки</param>
        /// <param name="dto">Данные с идентификатором назначаемого менеджера</param>
        /// <returns>Обновлённая заявка в формате ApplicationResponseDto</returns>
        /// <remarks>
        /// Права доступа:
        /// - Доступ имеет только Manager
        /// </remarks>
        /// <response code="200">Менеджер успешно назначен</response>
        /// <response code="400">Указанный пользователь не найден или не является менеджером</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="403">Текущий пользователь не имеет роли Manager</response>
        /// <response code="404">Заявка с указанным ID не найдена</response>
        [Authorize(Roles = "Manager")]
        [HttpPatch("{id}/assign")]
        [ProducesResponseType(typeof(ApplicationResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AssignApplication(Guid id, [FromBody] AssignApplicationRequestDto dto)
        {
            var application = await _context.Applications
                .Include(a => a.Direction)
                .Include(a => a.Format)
                .Include(a => a.Author)
                .FirstOrDefaultAsync(a => a.Id == id);

            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            Guid.TryParse(currentUserIdClaim?.Value, out Guid currentUserId);

            _logger.LogInformation("Пользователь {UserId} пытается назначить сотрудника {ManagerId} на заявку {ApplicationId}", currentUserId, dto.AssignedToId, id);

            if (application == null)
            {
                _logger.LogWarning("Назначение отклонено: Заявка {ApplicationId} не найдена.", id);
                return NotFound(new { message = "Заявка не найдена." });
            }

            var targetUser = await _context.Users.FindAsync(dto.AssignedToId);
            if (targetUser == null)
            {   
                _logger.LogWarning("Назначение отклонено: Пользователь {ManagerId} не существует.", dto.AssignedToId);
                return BadRequest(new { message = "Указанный пользователь не найден." });
            }

            if (!string.Equals(targetUser.Role, "Manager", StringComparison.OrdinalIgnoreCase))
            {   
                _logger.LogWarning("Назначение отклонено: Пользователь {ManagerId} не является менеджером.", dto.AssignedToId);
                return BadRequest(new { message = "Назначить ответственным можно только пользователя с ролью Manager." });
            }


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

            _logger.LogInformation("Заявка {ApplicationId} успешно назначена на менеджера {ManagerId} сотрудником {UserId}.", id, dto.AssignedToId, currentUserId);

            return Ok(responseDto);
        }

        /// <summary>
        /// Изменение статуса заявки менеджером
        /// </summary>
        /// <param name="id">Идентификатор заявки</param>
        /// <param name="dto">Новый статус заявки (например: New, InProgress, NeedsInfo, Approved, Rejected, Completed)</param>
        /// <returns>Обновлённая заявка в формате ApplicationResponseDto</returns>
        /// <remarks>
        /// Права доступа:
        /// - Доступ имеет только Manager
        /// </remarks>
        /// <response code="200">Статус успешно обновлён</response>
        /// <response code="400">Некорректный запрос: финальный статус без назначенного менеджера</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="403">Текущий пользователь не имеет роли Manager</response>
        /// <response code="404">Заявка с указанным ID не найдена</response>
        [Authorize(Roles = "Manager")]
        [HttpPatch("{id}/status")]
        [ProducesResponseType(typeof(ApplicationResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateApplicationStatusRequestDto dto)
        {
            var application = await _context.Applications
                .Include(a => a.Direction)
                .Include(a => a.Format)
                .Include(a => a.Author)
                .Include(a => a.AssignedTo)
                .FirstOrDefaultAsync(a => a.Id == id);

            var currentUserIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            Guid.TryParse(currentUserIdClaim?.Value, out Guid currentUserId);

            _logger.LogInformation("Пользователь {UserId} пытается изменить статус заявки {ApplicationId} на '{Status}'", currentUserId, id, dto.Status);
            
            if (application == null)
            {
                _logger.LogWarning("Изменение статуса отклонено: Заявка {ApplicationId} не найдена.", id);
                return NotFound(new { message = "Заявка не найдена." });
            }

            if ((string.Equals(dto.Status, "Approved", StringComparison.OrdinalIgnoreCase) || 
                 string.Equals(dto.Status, "Rejected", StringComparison.OrdinalIgnoreCase) || 
                 string.Equals(dto.Status, "Completed", StringComparison.OrdinalIgnoreCase)) && 
                application.AssignedToId == null)
            {
                _logger.LogWarning("Изменение статуса отклонено: Попытка перевода заявки {ApplicationId} в статус '{Status}' без назначенного менеджера.", id, dto.Status);
                return BadRequest(new { message = "Нельзя перевести заявку в финальный статус без назначенного ответственного менеджера." });
            }

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

            _logger.LogInformation("Статус заявки {ApplicationId} успешно изменен на '{Status}' пользователем {UserId}.", id, dto.Status, currentUserId);
            return Ok(responseDto);
        }

        /// <summary>
        /// Добавление комментария к заявке
        /// </summary>
        /// <param name="id">Идентификатор заявки</param>
        /// <param name="dto">Текст комментария</param>
        /// <returns>Созданный комментарий в формате CommentResponseDto</returns>
        /// <remarks>
        /// Права доступа:
        /// - Applicant может комментировать только свои заявки
        /// - Manager, Admin и Director могут комментировать любые заявки
        /// </remarks>
        /// <response code="200">Комментарий успешно добавлен</response>
        /// <response code="400">Некорректные данные запроса</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="403">Недостаточно прав для добавления комментария к этой заявке</response>
        /// <response code="404">Заявка с указанным ID не найдена</response>
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
                _logger.LogWarning("Добавление комментария: Не удалось определить пользователя из токена.");
                return Unauthorized(new { message = "Не удалось определить пользователя из токена." });
            }

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            _logger.LogInformation("Пользователь {UserId} пытается добавить комментарий к заявке {ApplicationId}", currentUserId, id);

            var application = await _context.Applications.FindAsync(id);
            if (application == null)
            {
                _logger.LogWarning("Добавление комментария: Заявка {ApplicationId} не найдена.", id);
                return NotFound(new { message = "Заявка не найдена." });
            }

            // Проверка прав: заявитель может комментировать только свою заявку
            if (userRole == "Applicant" && application.AuthorId != currentUserId)
            {
                _logger.LogWarning("Добавление комментария: Пользователь {UserId} не является автором заявки {ApplicationId}.", currentUserId, id);
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
            
            _logger.LogInformation("Комментарий успешно добавлен к заявке {ApplicationId} пользователем {UserId}", id, currentUserId);

            return Ok(responseDto);
        }

        /// <summary>
        /// Получение всех комментариев к заявке
        /// </summary>
        /// <param name="id">Идентификатор заявки</param>
        /// <returns>Список комментариев в формате CommentResponseDto</returns>
        /// <remarks>
        /// Права доступа:
        /// - Applicant может просматривать комментарии только к своей заявке
        /// - Manager, Admin и Director могут просматривать комментарии к любым заявкам
        /// </remarks>
        /// <response code="200">Успешное получение списка комментариев</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="403">Недостаточно прав для просмотра комментариев к этой заявке</response>
        /// <response code="404">Заявка с указанным ID не найдена</response>
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
                _logger.LogWarning("Получение комментариев: Не удалось определить пользователя из токена.");
                return Unauthorized(new { message = "Не удалось определить пользователя из токена." });
            }

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var application = await _context.Applications.FindAsync(id);
            if (application == null)
            {
                _logger.LogWarning("Получение комментариев: Заявка {ApplicationId} не найдена.", id);
                return NotFound(new { message = "Заявка не найдена." });
            }

            // Проверка прав: заявитель может видеть комментарии только к своей заявке
            if (userRole == "Applicant" && application.AuthorId != currentUserId)
            {
                _logger.LogWarning("Получение комментариев: Пользователь {UserId} не является автором заявки {ApplicationId}.", currentUserId, id);
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

            _logger.LogDebug("Получение комментариев: Успешно возвращено {Count} комментариев для заявки {ApplicationId}.", comments.Count, id);

            return Ok(comments);
        }
    }
}