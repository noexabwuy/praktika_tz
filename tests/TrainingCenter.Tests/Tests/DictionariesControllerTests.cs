using System;
using System.Collections.Generic;
using System.Linq;
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

namespace TrainingCenter.Tests
{
    public class DictionariesControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly DictionariesController _controller;

        public DictionariesControllerTests()
        {
            _context = TestDatabaseHelper.CreateAndSeedDictionaryDatabase();
            var loggerMock = new Mock<ILogger<DictionariesController>>();
            _controller = new DictionariesController(_context, loggerMock.Object);
            SetupUserWithRole("Admin");
        }

        private void SetupUserWithRole(string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, role),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // Сценарий: Авторизованный пользователь (Admin, Manager, Applicant, Director) запрашивает список всех направлений обучения
        // Ожидаемый результат: Возвращаются все 6 направлений из тестовой БД
        // Код ответа: 200 OK
        [Fact]
        public async Task GetDirections_ReturnsAllDirections()
        {
            // Act
            var result = await _controller.GetDirections();

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var directions = okResult.Value as List<DictionaryDto>;
            directions.Should().NotBeNull();
            directions!.Count.Should().Be(6);
            directions.All(d => !string.IsNullOrEmpty(d.Name)).Should().BeTrue();
        }

        // Сценарий: Авторизованный пользователь (Admin, Manager, Applicant, Director) запрашивает список всех форматов обучения
        // Ожидаемый результат: Возвращаются все 4 формата из тестовой БД
        // Код ответа: 200 OK
        [Fact]
        public async Task GetStudyFormats_ReturnsAllFormats()
        {
            // Act
            var result = await _controller.GetStudyFormats();

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var formats = okResult.Value as List<DictionaryDto>;
            formats.Should().NotBeNull();
            formats!.Count.Should().Be(4);
            formats.All(f => !string.IsNullOrEmpty(f.Name)).Should().BeTrue();
        }

        // Сценарий: Авторизованный пользователь (Admin, Manager, Applicant, Director) запрашивает список всех статусов заявок
        // Ожидаемый результат: Возвращаются 6 статусов: New, InProgress, NeedsInfo, Approved, Rejected, Completed
        // Код ответа: 200 OK
        [Fact]
        public void GetStatuses_ReturnsAllStatuses()
        {
            // Act
            var result = _controller.GetStatuses();

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var statuses = okResult.Value as List<DictionaryDto>;
            statuses.Should().NotBeNull();
            statuses!.Count.Should().Be(6);
            statuses.Select(s => s.Id).Should().Contain(new[] { "New", "InProgress", "NeedsInfo", "Approved", "Rejected", "Completed" });
        }

        // Сценарий: Администратор отправляет запрос на создание нового направления с уникальным названием
        // Ожидаемый результат: Направление успешно создаётся, возвращается созданный объект
        // Код ответа: 201
        [Fact]
        public async Task CreateDirection_WithValidData_ReturnsCreated()
        {
            // Arrange
            var dto = new DictionaryRequestDto { Name = "Новое направление" };

            // Act
            var result = await _controller.CreateDirection(dto);

            // Assert
            var createdResult = result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be(201);

            var response = createdResult.Value as DictionaryResponseDto;
            response.Should().NotBeNull();
            response!.Name.Should().Be("Новое направление");
        }

        // Сценарий: Администратор пытается создать направление с названием, которое уже существует в БД
        // Ожидаемый результат: Возвращается ошибка с сообщением "Направление с таким названием уже существует"
        // Код ответа: 400
        [Fact]
        public async Task CreateDirection_WithDuplicateName_ReturnsBadRequest()
        {
            // Arrange
            var dto = new DictionaryRequestDto { Name = "Программирование" };

            // Act
            var result = await _controller.CreateDirection(dto);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be(400);
        }

        // Сценарий: Администратор отправляет запрос с пустым телом (null)
        // Ожидаемый результат: Возвращается ошибка с сообщением о некорректных данных запроса
        // Код ответа: 400
        [Fact]
        public async Task CreateDirection_WithNullDto_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.CreateDirection(null!);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be(400);
        }

        // Сценарий: Администратор обновляет название существующего направления на уникальное
        // Ожидаемый результат: Название направления успешно обновляется, возвращается обновлённый объект
        // Код ответа: 200 OK
        [Fact]
        public async Task UpdateDirection_WithValidData_ReturnsOk()
        {
            // Arrange
            var directionId = Guid.Parse("10000000-0000-0000-0000-000000000001");
            var dto = new DictionaryRequestDto { Name = "Обновлённое программирование" };

            // Act
            var result = await _controller.UpdateDirection(directionId, dto);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var response = okResult.Value as DictionaryResponseDto;
            response.Should().NotBeNull();
            response!.Name.Should().Be("Обновлённое программирование");
        }

        // Сценарий: Администратор пытается обновить направление с несуществующим ID
        // Ожидаемый результат: Возвращается ошибка "Направление не найдено"
        // Код ответа: 404
        [Fact]
        public async Task UpdateDirection_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var dto = new DictionaryRequestDto { Name = "Несуществующее" };

            // Act
            var result = await _controller.UpdateDirection(nonExistentId, dto);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be(404);
        }

        // Сценарий: Администратор пытается обновить направление, указав название, которое уже занято другой записью
        // Ожидаемый результат: Возвращается ошибка "Направление с таким названием уже существует"
        // Код ответа: 400
        [Fact]
        public async Task UpdateDirection_WithDuplicateName_ReturnsBadRequest()
        {
            // Arrange
            var directionId = Guid.Parse("10000000-0000-0000-0000-000000000001");
            var dto = new DictionaryRequestDto { Name = "Дизайн" };

            // Act
            var result = await _controller.UpdateDirection(directionId, dto);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be(400);
        }

        // Сценарий: Администратор удаляет направление, которое не используется ни в одной заявке
        // Ожидаемый результат: Направление успешно удаляется
        // Код ответа: 200 OK
        [Fact]
        public async Task DeleteDirection_WithValidId_ReturnsOk()
        {
            // Arrange
            var directionId = Guid.Parse("10000000-0000-0000-0000-000000000004");

            // Act
            var result = await _controller.DeleteDirection(directionId);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
        }

        // Сценарий: Администратор пытается удалить направление с несуществующим ID
        // Ожидаемый результат: Возвращается ошибка "Направление не найдено"
        // Код ответа: 404
        [Fact]
        public async Task DeleteDirection_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _controller.DeleteDirection(nonExistentId);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be(404);
        }

        // Сценарий: Администратор пытается удалить направление, которое используется в существующих заявках
        // Ожидаемый результат: Возвращается ошибка "Нельзя удалить направление, так как оно используется в заявках"
        // Код ответа: 400
        [Fact]
        public async Task DeleteDirection_WithUsedInApplications_ReturnsBadRequest()
        {
            // Arrange
            var directionId = Guid.Parse("10000000-0000-0000-0000-000000000001");

            // Act
            var result = await _controller.DeleteDirection(directionId);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be(400);
        }

        // Сценарий: Администратор отправляет запрос на создание нового формата с уникальным названием
        // Ожидаемый результат: Формат успешно создаётся, возвращается созданный объект
        // Код ответа: 201
        [Fact]
        public async Task CreateTrainingFormat_WithValidData_ReturnsCreated()
        {
            // Arrange
            var dto = new DictionaryRequestDto { Name = "Новый формат" };

            // Act
            var result = await _controller.CreateTrainingFormat(dto);

            // Assert
            var createdResult = result as CreatedAtActionResult;
            createdResult.Should().NotBeNull();
            createdResult!.StatusCode.Should().Be(201);

            var response = createdResult.Value as DictionaryResponseDto;
            response.Should().NotBeNull();
            response!.Name.Should().Be("Новый формат");
        }

        // Сценарий: Администратор пытается создать формат с названием, которое уже существует в БД
        // Ожидаемый результат: Возвращается ошибка "Формат с таким названием уже существует"
        // Код ответа: 400
        [Fact]
        public async Task CreateTrainingFormat_WithDuplicateName_ReturnsBadRequest()
        {
            // Arrange
            var dto = new DictionaryRequestDto { Name = "Очный" };

            // Act
            var result = await _controller.CreateTrainingFormat(dto);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be(400);
        }

        // Сценарий: Администратор отправляет запрос на создание формата с пустым телом (null)
        // Ожидаемый результат: Возвращается ошибка с сообщением "Некорректные данные запроса"
        // Код ответа: 400
        [Fact]
        public async Task CreateTrainingFormat_WithNullDto_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.CreateTrainingFormat(null!);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be(400);
        }

        // Сценарий: Администратор обновляет название существующего формата на уникальное
        // Ожидаемый результат: Название формата успешно обновляется, возвращается обновлённый объект
        // Код ответа: 200 OK
        [Fact]
        public async Task UpdateTrainingFormat_WithValidData_ReturnsOk()
        {
            // Arrange
            var formatId = Guid.Parse("20000000-0000-0000-0000-000000000001");
            var dto = new DictionaryRequestDto { Name = "Обновлённый очный" };

            // Act
            var result = await _controller.UpdateTrainingFormat(formatId, dto);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);

            var response = okResult.Value as DictionaryResponseDto;
            response.Should().NotBeNull();
            response!.Name.Should().Be("Обновлённый очный");
        }

        // Сценарий: Администратор пытается обновить формат с несуществующим ID
        // Ожидаемый результат: Возвращается ошибка "Формат не найден"
        // Код ответа: 404
        [Fact]
        public async Task UpdateTrainingFormat_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var dto = new DictionaryRequestDto { Name = "Несуществующий" };

            // Act
            var result = await _controller.UpdateTrainingFormat(nonExistentId, dto);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be(404);
        }

        // Сценарий: Администратор пытается обновить формат, указав название, которое уже занято другой записью
        // Ожидаемый результат: Возвращается ошибка "Формат с таким названием уже существует"
        // Код ответа: 400
        [Fact]
        public async Task UpdateTrainingFormat_WithDuplicateName_ReturnsBadRequest()
        {
            // Arrange
            var formatId = Guid.Parse("20000000-0000-0000-0000-000000000001");
            var dto = new DictionaryRequestDto { Name = "Онлайн" };

            // Act
            var result = await _controller.UpdateTrainingFormat(formatId, dto);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be(400);
        }

        // Сценарий: Администратор удаляет формат, который не используется ни в одной заявке
        // Ожидаемый результат: Формат успешно удаляется
        // Код ответа: 200 OK
        [Fact]
        public async Task DeleteTrainingFormat_WithValidId_ReturnsOk()
        {
            // Arrange
            var formatId = Guid.Parse("20000000-0000-0000-0000-000000000003");

            // Act
            var result = await _controller.DeleteTrainingFormat(formatId);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
        }

        // Сценарий: Администратор пытается удалить формат с несуществующим ID
        // Ожидаемый результат: Возвращается ошибка "Формат не найден"
        // Код ответа: 404
        [Fact]
        public async Task DeleteTrainingFormat_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _controller.DeleteTrainingFormat(nonExistentId);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.StatusCode.Should().Be(404);
        }

        // Сценарий: Администратор пытается удалить формат, который используется в существующих заявках
        // Ожидаемый результат: Возвращается ошибка "Нельзя удалить формат, так как он используется в заявках"
        // Код ответа: 400 
        [Fact]
        public async Task DeleteTrainingFormat_WithUsedInApplications_ReturnsBadRequest()
        {
            // Arrange
            var formatId = Guid.Parse("20000000-0000-0000-0000-000000000001");

            // Act
            var result = await _controller.DeleteTrainingFormat(formatId);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be(400);
        }
    }
}