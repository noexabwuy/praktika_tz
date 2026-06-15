using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Models.DTOs;
using api.Models.Entities;

namespace api.Controllers
{
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

        // GET /api/dictionaries/directions - список направлений
        [HttpGet("directions")]
        [ProducesResponseType(typeof(List<DictionaryDto>), 200)]
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

        // GET /api/dictionaries/study-formats - список форматов
        [HttpGet("study-formats")]
        [ProducesResponseType(typeof(List<DictionaryDto>), 200)]
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

        // GET /api/dictionaries/statuses - список статусов
        [HttpGet("statuses")]
        [ProducesResponseType(typeof(List<DictionaryDto>), 200)]
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

        // POST	/api/dictionaries/directions - создать направление
        [HttpPost("directions")]
        [Authorize(Roles = "Admin")]  // Только для роли Admin
        [ProducesResponseType(typeof(DictionaryResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateDirection([FromBody] DictionaryRequestDto dto)
        {
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
                Id = direction.Id.ToString(),
                Name = direction.Name
            });
        }

        // PUT /api/dictionaries/directions/{id} - обновить направление
        [HttpPut("directions/{id}")]
        [Authorize(Roles = "Admin")]  // Только для роли Admin
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
                Id = direction.Id.ToString(),
                Name = direction.Name
            });
        }

        // DELETE /api/dictionaries/directions/{id} - удалить направление
        [HttpDelete("directions/{id}")]
        [Authorize(Roles = "Admin")]  // Только для роли Admin
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

        // POST /api/dictionaries/training-formats - создать формат
        [HttpPost("training-formats")]
        [Authorize(Roles = "Admin")]  // Только для роли Admin
        [ProducesResponseType(typeof(DictionaryResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> CreateTrainingFormat([FromBody] DictionaryRequestDto dto)
        {
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
                Id = format.Id.ToString(),
                Name = format.Name
            });
        }

        // PUT /api/dictionaries/training-formats/{id} - обновить формат
        [HttpPut("training-formats/{id}")]
        [Authorize(Roles = "Admin")]  // Только для роли Admin
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
                Id = format.Id.ToString(),
                Name = format.Name
            });
        }

        // DELETE /api/dictionaries/training-formats/{id} - удалить формат
        [HttpDelete("training-formats/{id}")]
        [Authorize(Roles = "Admin")]  // Только для роли Admin
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