using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Models.DTOs;
using api.Models.Entities;

namespace api.Controllers
{
    /// <summary>
    /// Контроллер для управления справочниками (направления, форматы обучения, статусы)
    /// </summary>
    [ApiController]
    [Route("api/dictionaries")]
    [Authorize] // Все эндпоинты требуют авторизации
    public class DictionariesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DictionariesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Получение списка всех направлений подготовки
        /// </summary>
        /// <returns>Список направлений в формате DictionaryDto</returns>
        /// <remarks>
        /// Права доступа:
        /// - Доступно всем авторизированным пользователям
        /// </remarks>
        /// <response code="200">Успешное получение списка направлений</response>
        /// <response code="401">Пользователь не авторизован</response>
        [HttpGet("directions")]
        [ProducesResponseType(typeof(List<DictionaryDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetDirections()
        {
            var directions = await _context.Directions
                .Select(d => new DictionaryDto
                {
                    Id = d.Id.ToString(),
                    Name = d.Name
                })
                .ToListAsync();

            return Ok(directions);
        }

        /// <summary>
        /// Получение списка всех форматов обучения
        /// </summary>
        /// <returns>Список форматов в формате DictionaryDto</returns>
        /// <remarks>
        /// Права доступа:
        /// - Доступно всем авторизированным пользователям
        /// </remarks>
        /// <response code="200">Успешное получение списка форматов</response>
        /// <response code="401">Пользователь не авторизован</response>
        [HttpGet("training-formats")]
        [ProducesResponseType(typeof(List<DictionaryDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetStudyFormats()
        {
            var formats = await _context.TrainingFormats
                .Select(f => new DictionaryDto
                {
                    Id = f.Id.ToString(),
                    Name = f.Name
                })
                .ToListAsync();

            return Ok(formats);
        }

        /// <summary>
        /// Получение списка всех возможных статусов заявок
        /// </summary>
        /// <returns>Список статусов в формате DictionaryDto</returns>
        /// <remarks>
        /// Права доступа:
        /// - Доступно всем авторизированным пользователям
        /// </remarks>
        /// <response code="200">Успешное получение списка статусов</response>
        /// <response code="401">Пользователь не авторизован</response>
        [HttpGet("statuses")]
        [ProducesResponseType(typeof(List<DictionaryDto>), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetStatuses()
        {
            var statuses = new List<DictionaryDto>
            {
                new DictionaryDto { Id = "New", Name = "Новая" },
                new DictionaryDto { Id = "InProgress", Name = "В работе" },
                new DictionaryDto { Id = "NeedsInfo", Name = "Требуется уточнение" },
                new DictionaryDto { Id = "Approved", Name = "Согласована" },
                new DictionaryDto { Id = "Rejected", Name = "Отклонена" },
                new DictionaryDto { Id = "Completed", Name = "Завершена" }
            };

            return Ok(statuses);
        }

        /// <summary>
        /// Создание нового направления подготовки
        /// </summary>
        /// <param name="dto">Данные для создания направления (название)</param>
        /// <returns>Созданное направление в формате DictionaryResponseDto</returns>
        /// <remarks>
        /// Права доступа:
        /// - Доступно только для Admin
        /// </remarks>
        /// <response code="201">Направление успешно создано</response>
        /// <response code="400">Некорректные данные запроса или направление с таким названием уже существует</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="403">Недостаточно прав (требуется роль Admin)</response>
        [HttpPost("directions")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(DictionaryResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateDirection([FromBody] DictionaryRequestDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Некорректные данные запроса" });
            }

            var exists = await _context.Directions
                .AnyAsync(d => d.Name.ToLower() == dto.Name.ToLower());

            if (exists)
            {
                return BadRequest(new { message = "Направление с таким названием уже существует" });
            }

            var direction = new Direction
            {
                Id = Guid.NewGuid(),
                Name = dto.Name
            };

            _context.Directions.Add(direction);
            await _context.SaveChangesAsync();

            return CreatedAtAction(null, new DictionaryResponseDto
            {
                Id = direction.Id,
                Name = direction.Name
            });
        }

        /// <summary>
        /// Обновление существующего направления подготовки
        /// </summary>
        /// <param name="id">Идентификатор направления</param>
        /// <param name="dto">Новые данные направления (название)</param>
        /// <returns>Обновлённое направление в формате DictionaryResponseDto</returns>
        /// <remarks>
        /// Права доступа:
        /// - Доступно только для Admin
        /// </remarks>
        /// <response code="200">Направление успешно обновлено</response>
        /// <response code="400">Некорректные данные запроса или направление с таким названием уже существует</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="403">Недостаточно прав (требуется роль Admin)</response>
        /// <response code="404">Направление с указанным ID не найдено</response>
        [HttpPut("directions/{id}")]
        [Authorize(Roles = "Admin")] 
        [ProducesResponseType(typeof(DictionaryResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateDirection(Guid id, [FromBody] DictionaryRequestDto dto)
        {
            var direction = await _context.Directions.FindAsync(id);

            if (direction == null)
            {
                return NotFound(new { message = "Направление не найдено" });
            }

            var exists = await _context.Directions
                .AnyAsync(d => d.Name.ToLower() == dto.Name.ToLower() && d.Id != id);

            if (exists)
            {
                return BadRequest(new { message = "Направление с таким названием уже существует" });
            }

            direction.Name = dto.Name;
            await _context.SaveChangesAsync();

            return Ok(new DictionaryResponseDto
            {
                Id = direction.Id,
                Name = direction.Name
            });
        }

        /// <summary>
        /// Удаление направления подготовки
        /// </summary>
        /// <param name="id">Идентификатор направления</param>
        /// <returns>Сообщение об успешном удалении</returns>
        /// <remarks>
        /// Права доступа:
        /// - Доступно только для Admin
        /// </remarks>
        /// <response code="200">Направление успешно удалено</response>
        /// <response code="400">Невозможно удалить направление, так как оно используется в заявках</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="403">Недостаточно прав (требуется роль Admin)</response>
        /// <response code="404">Направление с указанным ID не найдено</response>
        [HttpDelete("directions/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteDirection(Guid id)
        {
            var direction = await _context.Directions.FindAsync(id);

            if (direction == null)
            {
                return NotFound(new { message = "Направление не найдено" });
            }

            var hasApplications = await _context.Applications
                .AnyAsync(a => a.DirectionId == id);

            if (hasApplications)
            {
                return BadRequest(new { message = "Нельзя удалить направление, так как оно используется в заявках" });
            }

            _context.Directions.Remove(direction);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Направление успешно удалено" });
        }

        /// <summary>
        /// Создание нового формата обучения
        /// </summary>
        /// <param name="dto">Данные для создания формата (название)</param>
        /// <returns>Созданный формат в формате DictionaryResponseDto</returns>
        /// <remarks>
        /// Права доступа:
        /// - Доступно только для Admin
        /// </remarks>
        /// <response code="201">Формат успешно создан</response>
        /// <response code="400">Некорректные данные запроса или формат с таким названием уже существует</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="403">Недостаточно прав (требуется роль Admin)</response>
        [HttpPost("training-formats")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(DictionaryResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateTrainingFormat([FromBody] DictionaryRequestDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "Некорректные данные запроса" });
            }

            var exists = await _context.TrainingFormats
                .AnyAsync(f => f.Name.ToLower() == dto.Name.ToLower());

            if (exists)
            {
                return BadRequest(new { message = "Формат с таким названием уже существует" });
            }

            var format = new TrainingFormat
            {
                Id = Guid.NewGuid(),
                Name = dto.Name
            };

            _context.TrainingFormats.Add(format);
            await _context.SaveChangesAsync();

            return CreatedAtAction(null, new DictionaryResponseDto
            {
                Id = format.Id,
                Name = format.Name
            });
        }

        /// <summary>
        /// Обновление существующего формата обучения
        /// </summary>
        /// <param name="id">Идентификатор формата</param>
        /// <param name="dto">Новые данные формата (название)</param>
        /// <returns>Обновлённый формат в формате DictionaryResponseDto</returns>
        /// <remarks>
        /// Права доступа:
        /// - Доступно только для Admin
        /// </remarks>
        /// <response code="200">Формат успешно обновлён</response>
        /// <response code="400">Некорректные данные запроса или формат с таким названием уже существует</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="403">Недостаточно прав (требуется роль Admin)</response>
        /// <response code="404">Формат с указанным ID не найден</response>
        [HttpPut("training-formats/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(DictionaryResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateTrainingFormat(Guid id, [FromBody] DictionaryRequestDto dto)
        {
            var format = await _context.TrainingFormats.FindAsync(id);

            if (format == null)
            {
                return NotFound(new { message = "Формат не найден" });
            }

            var exists = await _context.TrainingFormats
                .AnyAsync(f => f.Name.ToLower() == dto.Name.ToLower() && f.Id != id);

            if (exists)
            {
                return BadRequest(new { message = "Формат с таким названием уже существует" });
            }

            format.Name = dto.Name;
            await _context.SaveChangesAsync();

            return Ok(new DictionaryResponseDto
            {
                Id = format.Id,
                Name = format.Name
            });
        }

        /// <summary>
        /// Удаление формата обучения (только для Admin)
        /// </summary>
        /// <param name="id">Идентификатор формата</param>
        /// <returns>Сообщение об успешном удалении</returns>
        /// <remarks>
        /// Права доступа:
        /// - Доступно только для Admin
        /// </remarks>
        /// <response code="200">Формат успешно удалён</response>
        /// <response code="400">Невозможно удалить формат, так как он используется в заявках</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="403">Недостаточно прав (требуется роль Admin)</response>
        /// <response code="404">Формат с указанным ID не найден</response>
        [HttpDelete("training-formats/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteTrainingFormat(Guid id)
        {
            var format = await _context.TrainingFormats.FindAsync(id);

            if (format == null)
            {
                return NotFound(new { message = "Формат не найден" });
            }

            var hasApplications = await _context.Applications
                .AnyAsync(a => a.FormatId == id);

            if (hasApplications)
            {
                return BadRequest(new { message = "Нельзя удалить формат, так как он используется в заявках" });
            }

            _context.TrainingFormats.Remove(format);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Формат успешно удален" });
        }
    }
}