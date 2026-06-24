using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using api.Controllers;
using api.Data;
using api.Models.DTOs;
using api.Models.Entities;
using TrainingCenter.Tests.Helpers;
using Moq;
using Microsoft.Extensions.Logging;

namespace TrainingCenter.Tests.Tests
{
    public class ApplicationsControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ApplicationsController _controller;

        // Идентификаторы тестовых пользователей из TestDatabaseHelper.cs
        private readonly Guid _applicantUserId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        private readonly Guid _managerUserId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        public ApplicationsControllerTests()
        {
            _context = TestDatabaseHelper.CreateAndSeedDatabase();
            var mockLogger = new Mock<ILogger<ApplicationsController>>();
            _controller = new ApplicationsController(_context, mockLogger.Object);
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

        private async Task<Application> SeedApplicationAsync(Guid authorId, string title, Guid? assignedToId = null)
        {
            var direction = new Direction { Id = Guid.NewGuid(), Name = "Тестовое направление" };
            var format = new TrainingFormat { Id = Guid.NewGuid(), Name = "Тестовый формат" };
            
            var application = new Application
            {
                Id = Guid.NewGuid(),
                Title = title,
                Description = "Описание тестовой заявки длиной более 10 символов",
                Status = "New",
                DirectionId = direction.Id,
                FormatId = format.Id,
                AuthorId = authorId,
                AssignedToId = assignedToId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Directions.Add(direction);
            _context.TrainingFormats.Add(format);
            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            return application;
        }

        // =========================================================================
        // 1. СЦЕНАРИИ GET /api/applications
        // =========================================================================

        [Fact]
        public async Task GetApplications_AsApplicant_WithMyTrue_ReturnsOnlyOwnApplications()
        {
            // Arrange
            SetupUserContext(_applicantUserId, "Applicant");
            await SeedApplicationAsync(_applicantUserId, "Моя заявка");
            await SeedApplicationAsync(_managerUserId, "Чужая заявка");

            // Act
            var result = await _controller.GetApplications(my: true, null, null, null, null, null);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var applications = okResult.Value as IEnumerable<ApplicationResponseDto>;
            applications.Should().NotBeNull();
            applications.Should().ContainSingle();
            applications.Should().Contain(a => a.Title == "Моя заявка");
        }

        [Fact]
        public async Task GetApplications_AsApplicant_WithMyFalse_ReturnsForbid()
        {
            // Arrange
            SetupUserContext(_applicantUserId, "Applicant");

            // Act
            var result = await _controller.GetApplications(my: false, null, null, null, null, null);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        // =========================================================================
        // 2. СЦЕНАРИИ POST /api/applications
        // =========================================================================

        [Fact]
        public async Task CreateApplication_WithValidData_ReturnsCreatedResult()
        {
            // Arrange
            SetupUserContext(_applicantUserId, "Applicant");

            var direction = new Direction { Id = Guid.NewGuid(), Name = "Разработка на .NET" };
            var format = new TrainingFormat { Id = Guid.NewGuid(), Name = "Дистанционный" };
            _context.Directions.Add(direction);
            _context.TrainingFormats.Add(format);
            await _context.SaveChangesAsync();

            var dto = new CreateApplicationRequestDto
            {
                Title = "Хочу на курс по C#",
                Description = "Изучаю backend-разразработку, хочу пройти практику.",
                DirectionId = direction.Id,
                FormatId = format.Id
            };

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var createdResult = result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be(201);

            var responseData = createdResult.Value as ApplicationResponseDto;
            responseData.Should().NotBeNull();
            responseData!.Title.Should().Be(dto.Title);
            responseData.AuthorId.Should().Be(_applicantUserId);
        }

        [Fact]
        public async Task CreateApplication_WithInvalidDirectionOrFormat_ReturnsBadRequest()
        {
            // Arrange
            SetupUserContext(_applicantUserId, "Applicant");

            var dto = new CreateApplicationRequestDto
            {
                Title = "Заявка в никуда",
                Description = "Описание корректной длины для прохождения валидации",
                DirectionId = Guid.NewGuid(), 
                FormatId = Guid.NewGuid()
            };

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be(400);
        }

        // =========================================================================
        // 3. СЦЕНАРИИ PATCH /api/applications/{id}/assign и /status
        // =========================================================================

        [Fact]
        public async Task AssignApplication_ValidManager_UpdatesStatusToInProgressAndSavesHistory()
        {
            // Arrange
            SetupUserContext(_managerUserId, "Manager");
            var application = await SeedApplicationAsync(_applicantUserId, "Заявка для назначения");

            var dto = new AssignApplicationRequestDto { AssignedToId = _managerUserId };

            // Act
            var result = await _controller.AssignApplication(application.Id, dto);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var responseData = okResult.Value as ApplicationResponseDto;
            responseData.Should().NotBeNull();
            responseData!.AssignedToId.Should().Be(_managerUserId);
            responseData.Status.Should().Be("InProgress");

            var history = await _context.StatusHistories.FirstOrDefaultAsync(h => h.ApplicationId == application.Id);
            history.Should().NotBeNull();
            history!.OldStatus.Should().Be("New");
            history.NewStatus.Should().Be("InProgress");
        }

        [Fact]
        public async Task UpdateStatus_ToFinalStatusWithoutManager_ReturnsBadRequest()
        {
            // Arrange
                SetupUserContext(_managerUserId, "Manager");
                var application = await SeedApplicationAsync(_applicantUserId, "Заявка без менеджера", assignedToId: null);

                var dto = new UpdateApplicationStatusRequestDto { Status = "Completed" };

                // Act
                var patchResult = await _controller.UpdateStatus(application.Id, dto);

                // Assert
                var badRequestResult = patchResult as BadRequestObjectResult;
                badRequestResult.Should().NotBeNull();
                badRequestResult!.StatusCode.Should().Be(400);
        }

        // =========================================================================
        // 4. СЦЕНАРИИ ДЛЯ КОММЕНТАРИЕВ (POST и GET)
        // =========================================================================

        [Fact]
        public async Task AddComment_OwnApplication_ReturnsOkWithComment()
        {
            // Arrange
            SetupUserContext(_applicantUserId, "Applicant");
            var application = await SeedApplicationAsync(_applicantUserId, "Моя заявка для комментов");

            var dto = new CreateCommentRequestDto { Text = "Новый тестовый комментарий" };

            // Act
            var result = await _controller.AddComment(application.Id, dto);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var responseData = okResult.Value as CommentResponseDto;
            responseData.Should().NotBeNull();
            responseData!.Text.Should().Be(dto.Text);
            responseData.AuthorId.Should().Be(_applicantUserId);
        }

        [Fact]
        public async Task AddComment_ForeignApplicationAsApplicant_ReturnsForbid()
        {
            // Arrange
            SetupUserContext(_applicantUserId, "Applicant");
            // Создаем заявку, где автором является Менеджер (чужая для Заявителя)
            var application = await SeedApplicationAsync(_managerUserId, "Чужая заявка");

            var dto = new CreateCommentRequestDto { Text = "Попытка оставить комментарий" };

            // Act
            var result = await _controller.AddComment(application.Id, dto);

            // Assert
            result.Should().BeOfType<ForbidResult>();
        }

        [Fact]
        public async Task GetComments_AsApplicantOnOwnApplication_ReturnsCommentsList()
        {
            // Arrange
            SetupUserContext(_applicantUserId, "Applicant");
            var application = await SeedApplicationAsync(_applicantUserId, "Заявка с комментариями");

            // Имитируем добавление комментария в БД
            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                ApplicationId = application.Id,
                AuthorId = _applicantUserId,
                Text = "Существующий комментарий",
                CreatedAt = DateTime.UtcNow
            };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetComments(application.Id);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var comments = okResult.Value as List<CommentResponseDto>;
            comments.Should().NotBeNull();
            comments.Should().NotBeEmpty();
            comments![0].Text.Should().Be("Существующий комментарий");
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}