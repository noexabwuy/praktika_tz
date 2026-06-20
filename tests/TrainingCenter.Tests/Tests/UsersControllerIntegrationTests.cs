using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using api.Data;
using api.Models.DTOs;
using api.Models.Entities;
using TrainingCenter.Tests.Helpers;

namespace TrainingCenter.Tests.Tests
{
    public class UsersControllerIntegrationTests : IDisposable
    {
        private readonly ApplicationDbContext _context;

        public UsersControllerIntegrationTests()
        {
            _context = TestDatabaseHelper.CreateAndSeedDatabase();
        }

        // Сценарий: Администратор логинится, получает токен, вызывает API с фильтром ?role=Admin
        // Ожидаемый результат: Возвращается 1 пользователь с ролью Admin
        // Код ответа: 200 OK
        [Fact]
        public async Task GetUsers_WithAdminToken_AndFilterByRole_ReturnsFilteredUsers()
        {
            // Arrange
            var token = await TestAuthHelper.Login(
                _context,
                "admin",
                "admin123"
            );

            // Act
            var result = await TestUsersControllerHelper.GetUsers(_context, token, "Admin");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();

            var users = okResult?.Value as List<UserResponseDto>;
            users.Should().NotBeNull();
            users!.Count.Should().Be(1);
            users[0].Role.Should().Be("Admin");
        }

        // Сценарий: Директор логинится, получает токен, вызывает API с фильтром ?role=Director
        // Ожидаемый результат: Возвращается 1 пользователь с ролью Director
        // Код ответа: 200 OK
        [Fact]
        public async Task GetUsers_WithDirectorToken_AndFilterByRole_ReturnsFilteredUsers()
        {
            // Arrange
            var token = await TestAuthHelper.Login(
                _context,
                "director",
                "director123"
            );

            // Act
            var result = await TestUsersControllerHelper.GetUsers(_context, token, "Director");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();

            var users = okResult?.Value as List<UserResponseDto>;
            users.Should().NotBeNull();
            users!.Count.Should().Be(1);
            users[0].Role.Should().Be("Director");
        }

        // Сценарий: Менеджер логинится, получает токен, вызывает API с фильтром ?role=Manager
        // Ожидаемый результат: Возвращается 1 пользователь с ролью Manager
        // Код ответа: 200 OK
        [Fact]
        public async Task GetUsers_WithManagerToken_AndFilterByRole_ReturnsFilteredUsers()
        {
            // Arrange
            var token = await TestAuthHelper.Login(
                _context,
                "ivanov",
                "123"
            );

            // Act
            var result = await TestUsersControllerHelper.GetUsers(_context, token, "Manager");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();

            var users = okResult?.Value as List<UserResponseDto>;
            users.Should().NotBeNull();
            users!.Count.Should().Be(1);
            users[0].Role.Should().Be("Manager");
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}