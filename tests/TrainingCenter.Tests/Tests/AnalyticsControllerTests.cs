using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using FluentAssertions;
using api.Controllers;
using api.Data;
using api.Models.DTOs;
using api.Models.Entities;
using TrainingCenter.Tests.Helpers;

namespace TrainingCenter.Tests.Tests
{
    public class AnalyticsControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly AnalyticsController _controller;

        private readonly Guid _adminUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        public AnalyticsControllerTests()
        {
            _context = TestDatabaseHelper.CreateAndSeedDatabase();
            _controller = new AnalyticsController(_context);
        }

        private void SetupUserContext(Guid userId, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task GetStatistics_AsAdmin_ReturnsValidAnalyticsDtoWithData()
        {
            // Arrange
            SetupUserContext(_adminUserId, "Admin");

            var direction = new Direction { Id = Guid.NewGuid(), Name = "Разработка C#" };
            var format = new TrainingFormat { Id = Guid.NewGuid(), Name = "Очный" };
            _context.Directions.Add(direction);
            _context.TrainingFormats.Add(format);

            var applicant = new User
            {
                Id = Guid.NewGuid(),
                FullName = "Студент Тестовый",
                Login = "stud_test",
                Email = "stud@test.com",
                PasswordHash = "hash",
                Role = "Applicant",
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(applicant);

            var application = new Application
            {
                Id = Guid.NewGuid(),
                Title = "Заявка на C#",
                Description = "Описание учебного процесса",
                Status = "New",
                DirectionId = direction.Id,
                FormatId = format.Id,
                AuthorId = applicant.Id,
                CreatedAt = DateTime.UtcNow
            };
            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetStatistics();

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var stats = okResult.Value as AnalyticsStatisticsDto;
            stats.Should().NotBeNull();
            
            // Проверяем агрегации
            stats!.TotalApplications.Should().BeGreaterThanOrEqualTo(1);
            stats.TotalUsers.Should().BeGreaterThanOrEqualTo(1);
            stats.ByStatuses.Should().ContainKey("New");
            stats.ByDirections.Should().ContainKey("Разработка C#");
            stats.ByFormats.Should().ContainKey("Очный");
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}