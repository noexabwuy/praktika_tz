using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Models.DTOs;

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

        [HttpGet("service-types")]
        public async Task<IActionResult> GetServiceTypes()
        {
            var items = await _context.ServiceTypes.ToListAsync();
            return Ok(items.Select(x => new ServiceTypeDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description
            }));
        }

        [HttpGet("statuses")]
        public async Task<IActionResult> GetStatuses()
        {
            var items = await _context.ApplicationStatuses.OrderBy(x => x.SortOrder).ToListAsync();
            return Ok(items.Select(x => new ApplicationStatusDto
            {
                Id = x.Id,
                Name = x.Name,
                Color = x.Color,
                SortOrder = x.SortOrder
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
