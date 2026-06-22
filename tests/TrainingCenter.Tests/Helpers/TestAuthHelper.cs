using System;
using System.Threading.Tasks;
using api.Controllers;
using api.Data;
using api.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Microsoft.Extensions.Logging;

namespace TrainingCenter.Tests.Helpers
{
    public static class TestAuthHelper
    {
        public static async Task<string> Login(
            ApplicationDbContext context,
                string login,
                string password)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    {"JwtSettings:Secret", "TestSecretKey_1234567890_1234567890_1234567890_1234567890"},
                    {"JwtSettings:Issuer", "TestIssuer"},
                    {"JwtSettings:Audience", "TestAudience"}
                })
                .Build();


            var mockLogger = new Mock<ILogger<AuthController>>();
            var authController = new AuthController(context, configuration, mockLogger.Object);

            var loginDto = new LoginRequestDto
            {
                Login = login,
                Password = password
            };

            var loginResult = await authController.Login(loginDto);
            if (loginResult is OkObjectResult okResult)
            {
                var response = okResult.Value as AuthResponseDto;
                return response?.Token ?? throw new Exception("Токен отсутствует.");
            }

            throw new Exception("Не удалось получить токен.");
        }
    }
}