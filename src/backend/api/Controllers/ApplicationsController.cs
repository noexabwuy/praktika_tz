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
    }
}