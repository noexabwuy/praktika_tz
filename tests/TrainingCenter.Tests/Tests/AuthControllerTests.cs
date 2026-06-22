using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;
using FluentAssertions;
using api.Controllers;
using api.Data;
using api.Models.DTOs;
using api.Models.Entities;
using TrainingCenter.Tests.Helpers;

namespace TrainingCenter.Tests.Tests
{
    public class AuthControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _context = TestDatabaseHelper.CreateAndSeedDatabase();

            // Создаем мок-конфигурацию с необходимыми для генерации JWT ключами
            var inMemorySettings = new Dictionary<string, string?> {
                {"JwtSettings:Secret", "SuperSecretKeyLongEnoughToSatisfyHmacSha256Requirement"},
                {"JwtSettings:Issuer", "TestIssuer"},
                {"JwtSettings:Audience", "TestAudience"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _controller = new AuthController(_context, _configuration);
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsOkAndCreatesUserWithDefaultRole()
        {
            // Arrange
            var dto = new RegisterRequestDto
            {
                FullName = "Тестовый Студент",
                Login = "teststudent",
                Email = "student@test.com",
                Password = "SuperSecurePassword123"
            };

            // Act
            var result = await _controller.Register(dto);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            // Проверяем физическое наличие в БД
            var dbUser = await _context.Users.FirstOrDefaultAsync(u => u.Login == dto.Login);
            dbUser.Should().NotBeNull();
            dbUser!.Role.Should().Be("Applicant");
        }

        [Fact]
        public async Task Register_WithExistingLogin_ReturnsBadRequest()
        {
            // Arrange
            // Создаем и сохраняем пользователя в БД, чтобы занять логин
            var existingUser = new User
            {
                Id = Guid.NewGuid(),
                FullName = "Существующий",
                Login = "occupied_login",
                Email = "old@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("anyPassword"),
                Role = "Applicant",
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var dto = new RegisterRequestDto
            {
                FullName = "Новый юзер",
                Login = "occupied_login", // Дубликат логина
                Email = "new@test.com",
                Password = "Password123"
            };

            // Act
            var result = await _controller.Register(dto);

            // Assert
            var badRequest = result as BadRequestObjectResult;
            badRequest.Should().NotBeNull();
            badRequest!.StatusCode.Should().Be(400);
        }

        [Fact]
        public async Task Login_WithCorrectCredentials_ReturnsOkWithJwtToken()
        {
            // Arrange
            var plainPassword = "CorrectPassword123";
            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = "Иван Иванов",
                Login = "ivanov_login",
                Email = "ivanov@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword),
                Role = "Applicant",
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var dto = new LoginRequestDto
            {
                Login = "ivanov_login",
                Password = plainPassword
            };

            // Act
            var result = await _controller.Login(dto);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var authResponse = okResult.Value as AuthResponseDto;
            authResponse.Should().NotBeNull();
            authResponse!.Token.Should().NotBeNullOrEmpty();
            authResponse.User.Login.Should().Be(user.Login);
        }

        [Fact]
        public async Task Login_WithWrongPassword_ReturnsUnauthorized()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = "Петр Петров",
                Login = "petrov_login",
                Email = "petrov@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("RealPassword123"),
                Role = "Applicant",
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var dto = new LoginRequestDto
            {
                Login = "petrov_login",
                Password = "WrongPassword!!!"
            };

            // Act
            var result = await _controller.Login(dto);

            // Assert
            var unauthorized = result as UnauthorizedObjectResult;
            unauthorized.Should().NotBeNull();
            unauthorized!.StatusCode.Should().Be(401);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}