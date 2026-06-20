using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using FluentAssertions;
using api.Controllers;
using api.Data;
using api.Models.DTOs;
using api.Models.Entities;
using System.Data;
using TrainingCenter.Tests.Helpers;

namespace TrainingCenter.Tests
{
    public class UsersControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            // Используем Helper для создания и заполнения БД
            _context = TestDatabaseHelper.CreateAndSeedDatabase();

            _controller = new UsersController(_context);

            // Настраиваем авторизацию (пользователь с ролью Admin)
            SetupUserWithRole("Admin");
        }

        private void SetupUserWithRole(string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.NameIdentifier, "11111111-1111-1111-1111-111111111111")
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        // Сценарий: Запрос без фильтра по роли(role не передан)
        // Ожидаемый результат: Возвращаются все 4 пользователя из тестовой БД
        // Код ответа: 200 OK
        [Fact]
        public async Task GetUsers_WithoutRoleFilter_ReturnsAllUsers()
        {
            // Act
            var result = await _controller.GetUsers(null);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var users = okResult.Value as List<UserResponseDto>;
            users.Should().NotBeNull();
            users!.Count.Should().Be(4);
        }

        // Сценарий: Запрос с фильтром ?role=Admin
        // Ожидаемый результат: Возвращается только 1 пользователь с ролью Admin
        // Код ответа: 200 OK
        [Fact]
        public async Task GetUsers_WithRoleFilterAdmin_ReturnsOnlyAdmins()
        {
            // Act
            var result = await _controller.GetUsers("Admin");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var users = okResult.Value as List<UserResponseDto>;
            users.Should().NotBeNull();
            users!.Count.Should().Be(1);
            users.All(u => u.Role == "Admin").Should().BeTrue();
        }

        // Сценарий: Запрос с фильтром ?role=Manager
        // Ожидаемый результат: Возвращается только 1 пользователь с ролью Manager
        // Код ответа: 200 OK
        [Fact]
        public async Task GetUsers_WithRoleFilterManager_ReturnsOnlyManagers()
        {
            // Act
            var result = await _controller.GetUsers("Manager");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var users = okResult.Value as List<UserResponseDto>;
            users.Should().NotBeNull();
            users!.Count.Should().Be(1);
            users.All(u => u.Role == "Manager").Should().BeTrue();
        }

        // Сценарий: Запрос с фильтром ?role=InvalidRole
        // Ожидаемый результат: Ошибка 400
        // Код ответа: 400
        [Fact]
        public async Task GetUsers_WithInvalidRole_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.GetUsers("InvalidRole");

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be(400);
        }

        // Сценарий: Запрос с фильтром ?role=admin (маленькими буквами)
        // Ожидаемый результат: Возвращается пользователь с ролью Admin (регистронезависимо)
        // Код ответа: 200 OK
        [Fact]
        public async Task GetUsers_WithRoleFilterCaseInsensitive_WorksCorrectly()
        {
            // Act
            var result = await _controller.GetUsers("admin");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var users = okResult.Value as List<UserResponseDto>;
            users.Should().NotBeNull();
            users!.Count.Should().Be(1);
            users!.All(u => u.Role == "Admin").Should().BeTrue();
        }

        // Сценарий: Запрос с фильтром ?role=Applicant
        // Ожидаемый результат: Возвращается только 1 пользователь с ролью Applicant
        // Код ответа: 200 OK
        [Fact]
        public async Task GetUsers_WithRoleFilterApplicant_ReturnsOnlyApplicants()
        {
            // Act
            var result = await _controller.GetUsers("Applicant");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var users = okResult.Value as List<UserResponseDto>;
            users.Should().NotBeNull();
            users!.Count.Should().Be(1);
            users.All(u => u.Role == "Applicant").Should().BeTrue();
        }

        // Сценарий: Запрос с фильтром ?role=Director
        // Ожидаемый результат: Возвращается только 1 пользователь с ролью Director
        // Код ответа: 200 OK
        [Fact]
        public async Task GetUsers_WithRoleFilterDirector_ReturnsOnlyDirectors()
        {
            // Act
            var result = await _controller.GetUsers("Director");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var users = okResult.Value as List<UserResponseDto>;
            users.Should().NotBeNull();
            users!.Count.Should().Be(1);
            users.All(u => u.Role == "Director").Should().BeTrue();
        }

        // Сценарий: Запрос к пустой базе данных (нет пользователей)
        // Ожидаемый результат: Возвращается пустой список
        // Код ответа: 200 OK
        [Fact]
        public async Task GetUsers_WhenNoUsers_ReturnsEmptyList()
        {
            // Arrange: создаем пустой контекст
            var emptyOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var emptyContext = new ApplicationDbContext(emptyOptions);
            var emptyController = new UsersController(emptyContext);
            SetupUserWithRole("Admin");

            // Act
            var result = await emptyController.GetUsers(null);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var users = okResult.Value as List<UserResponseDto>;
            users.Should().NotBeNull();
            users!.Count.Should().Be(0);
        }
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}