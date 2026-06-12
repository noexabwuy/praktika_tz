using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Models.DTOs;
using api.Models.Entities;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReferenceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReferenceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("directions")]
        public async Task<IActionResult> GetDirections()
        {
            var items = await _context.Directions.ToListAsync();
            return Ok(items.Select(x => new DirectionDto
            {
                Id = x.Id,
                Name = x.Name
            }));
        }

        [HttpGet("training-formats")]
        public async Task<IActionResult> GetTrainingFormats()
        {
            var items = await _context.TrainingFormats.ToListAsync();
            return Ok(items.Select(x => new TrainingFormatDto
            {
                Id = x.Id,
                Name = x.Name
            }));
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var items = await _context.Users.ToListAsync();
            return Ok(items.Select(x => new UserDto
            {
                Id = x.Id,
                FullName = x.FullName,
                Login = x.Login,
                Email = x.Email,
                Role = x.Role
            }));
        }
    }
}