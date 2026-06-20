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
    /// <summary>
    /// Контроллер для получения аналитической информации по заявкам и пользователям
    /// </summary>
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

        /// <summary>
        /// Получение сводной статистики по заявкам и пользователям
        /// </summary>
        /// <returns>Объект AnalyticsStatisticsDto со статистическими данными</returns>
        /// <remarks>
        /// Права доступа:
        /// - Просмотр статистики доступен только Admin и Director
        /// 
        /// Возвращает следующие данные:
        /// - Общее количество заявок
        /// - Общее количество зарегистрированных пользователей
        /// - Распределение заявок по статусам
        /// - Распределение заявок по направлениям подготовки
        /// - Распределение заявок по форматам обучения
        /// </remarks>
        /// <response code="200">Успешное получение статистических данных</response>
        /// <response code="401">Пользователь не авторизован</response>
        /// <response code="403">Недостаточно прав (требуется роль Director или Admin)</response>
        [HttpGet("statistics")]
        [ProducesResponseType(typeof(AnalyticsStatisticsDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        public async Task<IActionResult> GetStatistics()
        {
            // 1. Подсчет общего количества заявок и зарегистрированных пользователей
            var totalApplications = await _context.Applications.CountAsync();
            var totalUsers = await _context.Users.CountAsync();

            // 2. Агрегация по статусам заявок
            var statusGroups = await _context.Applications
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            // 3. Агрегация по направлениям обучения
            var directionGroups = await _context.Applications
                .GroupBy(a => a.Direction.Name)
                .Select(g => new { DirectionName = g.Key, Count = g.Count() })
                .ToListAsync();

            // 4. Агрегация по форматам обучения (Training Formats)
            var formatGroups = await _context.Applications
                .GroupBy(a => a.Format.Name)
                .Select(g => new { FormatName = g.Key, Count = g.Count() })
                .ToListAsync();

            // Преобразуем сгруппированные данные в словари для результирующего JSON
            var byStatuses = statusGroups.ToDictionary(x => x.Status, x => x.Count);
            var byDirections = directionGroups.ToDictionary(x => x.DirectionName, x => x.Count);
            var byFormats = formatGroups.ToDictionary(x => x.FormatName, x => x.Count);

            var statistics = new AnalyticsStatisticsDto
            {
                TotalApplications = totalApplications,
                TotalUsers = totalUsers,
                ByStatuses = byStatuses,
                ByDirections = byDirections,
                ByFormats = byFormats
            };

            return Ok(statistics);
        }
    }
}