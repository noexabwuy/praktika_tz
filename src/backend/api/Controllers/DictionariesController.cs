using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Models.DTOs;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DictionariesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DictionariesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Эндпоинт 1 - получить список направлений обучения
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

        // Эндпоинт 2 - получить список форматов обучения
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

        // Эндпоинт 3 - получить список статусов заявок
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
    }
}