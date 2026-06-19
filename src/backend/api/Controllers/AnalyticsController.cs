using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Models.DTOs;

namespace api.Controllers
{
    [ApiController]
    [Route("api/analytics")]
    [Authorize(Roles = "Director,Admin")]
    public class AnalyticsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("statistics")]
        [ProducesResponseType(typeof(AnalyticsStatisticsDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetStatistics()
        {

            var totalApplications = await _context.Applications.CountAsync();

            // Агрегация по статусам на уровне СУБД
            var statusGroups = await _context.Applications
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            
            var directionGroups = await _context.Applications
                .GroupBy(a => a.Direction.Name)
                .Select(g => new { DirectionName = g.Key, Count = g.Count() })
                .ToListAsync();

            
            var byStatuses = statusGroups.ToDictionary(x => x.Status, x => x.Count);
            var byDirections = directionGroups.ToDictionary(x => x.DirectionName, x => x.Count);

            var statistics = new AnalyticsStatisticsDto
            {
                TotalApplications = totalApplications,
                ByStatuses = byStatuses,
                ByDirections = byDirections
            };

            return Ok(statistics);
        }
    }
}