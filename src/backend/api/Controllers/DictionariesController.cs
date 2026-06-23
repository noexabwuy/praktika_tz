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
    /// Контроллер для управления справочниками (направления, форматы обучения, статусы)
    /// </summary>
    [ApiController]
    [Route("api/dictionaries")]
    [Authorize]
    public class DictionariesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DictionariesController>? _logger;

        public DictionariesController(ApplicationDbContext context, ILogger<DictionariesController>? logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("directions")]
        [ProducesResponseType(typeof(List<DictionaryDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetDirections()
        {
            _logger?.LogInformation("Получение направлений: Запрошен список всех направлений подготовки.");
            var directions = await _context.Directions
                .Select(d => new DictionaryDto
                {
                    Id = d.Id.ToString(),
                    Name = d.Name
                })
                .ToListAsync();

            return Ok(directions);
        }

        [HttpGet("training-formats")]
        [ProducesResponseType(typeof(List<DictionaryDto>), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetStudyFormats()
        {
            _logger?.LogInformation("Получение форматов обучения: Запрошен список всех форматов обучения.");
            var formats = await _context.TrainingFormats
                .Select(f => new DictionaryDto
                {
                    Id = f.Id.ToString(),
                    Name = f.Name
                })
                .ToListAsync();

            return Ok(formats);
        }

        [HttpGet("statuses")]
        [ProducesResponseType(typeof(List<DictionaryDto>), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetStatuses()
        {
            _logger?.LogInformation("Получение статусов: Запрошен список всех статусов.");

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

        [HttpPost("directions")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(DictionaryResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateDirection([FromBody] DictionaryRequestDto? dto)
        {
            if (dto == null)
            {
                _logger?.LogWarning("Создание направления: Отклонено, некорректные данные запроса (null DTO).");
                return BadRequest(new { message = "Некорректные данные запроса" });
            }

            _logger?.LogInformation("Создание направления: Попытка создания направления с названием '{Name}'", dto.Name);

            var nameToCheck = (dto.Name ?? string.Empty).ToLowerInvariant();
            var exists = await _context.Directions
                .AnyAsync(d => d.Name.ToLower() == nameToCheck);

            if (exists)
            {
                _logger?.LogWarning("Создание направления: Отклонено, направление '{Name}' уже существует.", dto.Name);
                return BadRequest(new { message = "Направление с таким названием уже существует" });
            }

            var direction = new Direction
            {
                Id = Guid.NewGuid(),
                Name = dto.Name ?? string.Empty
            };

            _context.Directions.Add(direction);
            await _context.SaveChangesAsync();

            _logger?.LogInformation("Создание направления: Успешно создано новое направление с ID {Id}", direction.Id);

            return CreatedAtAction(null, new DictionaryResponseDto
            {
                Id = direction.Id,
                Name = direction.Name
            });
        }

        [HttpPut("directions/{id}")]
        [Authorize(Roles = "Admin")] 
        [ProducesResponseType(typeof(DictionaryResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateDirection(Guid id, [FromBody] DictionaryRequestDto? dto)
        {
            var direction = await _context.Directions.FindAsync(id);

            if (direction == null)
            {
                _logger?.LogWarning("Обновление направления: Направление с ID {Id} не найдено.", id);
                return NotFound(new { message = "Направление не найдено" });
            }

            if (dto == null)
            {
                _logger?.LogWarning("Обновление направления: Отклонено, некорректные данные запроса (null DTO).");
                return BadRequest(new { message = "Некорректные данные запроса" });
            }

            var nameToCheck = (dto.Name ?? string.Empty).ToLowerInvariant();
            var exists = await _context.Directions
                .AnyAsync(d => d.Name.ToLower() == nameToCheck && d.Id != id);

            if (exists)
            {
                _logger?.LogWarning("Обновление направления: Отклонено, направление '{Name}' уже существует.", dto.Name);
                return BadRequest(new { message = "Направление с таким названием уже существует" });
            }

            direction.Name = dto.Name ?? string.Empty;
            await _context.SaveChangesAsync();

            _logger?.LogInformation("Обновление направления: Направление с ID {Id} успешно обновлено.", id);

            return Ok(new DictionaryResponseDto
            {
                Id = direction.Id,
                Name = direction.Name
            });
        }

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
                _logger?.LogWarning("Удаление направления: Направление с ID {Id} не найдено.", id);
                return NotFound(new { message = "Направление не найдено" });
            }

            var hasApplications = await _context.Applications
                .AnyAsync(a => a.DirectionId == id);

            if (hasApplications)
            {
                _logger?.LogWarning("Удаление направления: Направление с ID {Id} используется в заявках.", id);
                return BadRequest(new { message = "Нельзя удалить направление, так как оно используется в заявках" });
            }

            _context.Directions.Remove(direction);
            await _context.SaveChangesAsync();

            _logger?.LogInformation("Удаление направления: Направление с ID {Id} успешно удалено.", id);
            return Ok(new { message = "Направление успешно удалено" });
        }

        [HttpPost("training-formats")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(DictionaryResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateTrainingFormat([FromBody] DictionaryRequestDto? dto)
        {
            if (dto == null)
            {
                _logger?.LogWarning("Создание формата обучения: Отклонено, некорректные данные запроса (null DTO).");
                return BadRequest(new { message = "Некорректные данные запроса" });
            }

            _logger?.LogInformation("Создание формата обучения: Попытка создания формата с названием '{Name}'", dto.Name);

            var nameToCheck = (dto.Name ?? string.Empty).ToLowerInvariant();
            var exists = await _context.TrainingFormats
                .AnyAsync(f => f.Name.ToLower() == nameToCheck);

            if (exists)
            {
                _logger?.LogWarning("Создание формата обучения: Отклонено, формат '{Name}' уже существует.", dto.Name);
                return BadRequest(new { message = "Формат с таким названием уже существует" });
            }

            var format = new TrainingFormat
            {
                Id = Guid.NewGuid(),
                Name = dto.Name ?? string.Empty
            };

            _context.TrainingFormats.Add(format);
            await _context.SaveChangesAsync();

            _logger?.LogInformation("Создание формата обучения: Успешно создан новый формат с ID {Id}", format.Id);

            return CreatedAtAction(null, new DictionaryResponseDto
            {
                Id = format.Id,
                Name = format.Name
            });
        }

        [HttpPut("training-formats/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(DictionaryResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateTrainingFormat(Guid id, [FromBody] DictionaryRequestDto? dto)
        {
            var format = await _context.TrainingFormats.FindAsync(id);

            if (format == null)
            {
                _logger?.LogWarning("Обновление формата обучения: Формат с ID {Id} не найден.", id);
                return NotFound(new { message = "Формат не найден" });
            }

            if (dto == null)
            {
                _logger?.LogWarning("Обновление формата обучения: Отклонено, некорректные данные запроса (null DTO).");
                return BadRequest(new { message = "Некорректные данные запроса" });
            }

            var nameToCheck = (dto.Name ?? string.Empty).ToLowerInvariant();
            var exists = await _context.TrainingFormats
                .AnyAsync(f => f.Name.ToLower() == nameToCheck && f.Id != id);

            if (exists)
            {
                _logger?.LogWarning("Обновление формата обучения: Отклонено, формат '{Name}' уже существует.", dto.Name);
                return BadRequest(new { message = "Формат с таким названием уже существует" });
            }

            format.Name = dto.Name ?? string.Empty;
            _logger?.LogInformation("Обновление формата обучения: Формат с ID {Id} успешно обновлен.", id);
            await _context.SaveChangesAsync();

            return Ok(new DictionaryResponseDto
            {
                Id = format.Id,
                Name = format.Name
            });
        }

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
                _logger?.LogWarning("Удаление формата обучения: Формат с ID {Id} не найден.", id);
                return NotFound(new { message = "Формат не найден" });
            }

            var hasApplications = await _context.Applications
                .AnyAsync(a => a.FormatId == id);

            if (hasApplications)
            {
                _logger?.LogWarning("Удаление формата обучения: Формат с ID {Id} используется в заявках.", id);
                return BadRequest(new { message = "Нельзя удалить формат, так как он используется в заявках" });
            }

            _context.TrainingFormats.Remove(format);
            await _context.SaveChangesAsync();

            _logger?.LogInformation("Удаление формата обучения: Формат с ID {Id} успешно удалён.", id);
            
            return Ok(new { message = "Формат успешно удален" });
        }
    }
}