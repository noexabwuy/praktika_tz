using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Models.Entities;

namespace TrainingCenter.Tests.Helpers
{
    public static class TestDatabaseHelper
    {
        public static ApplicationDbContext CreateAndSeedDatabase()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);
            SeedDatabase(context);
            return context;
        }

        public static void SeedDatabase(ApplicationDbContext context)
        {
            var users = new List<User>
            {
                new User
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    FullName = "Директор",
                    Login = "director",
                    Email = "director@training.ru",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("director123"),
                    Role = "Director",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    FullName = "Администратор",
                    Login = "admin",
                    Email = "admin@training.ru",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = "Admin",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    FullName = "Иванов Иван",
                    Login = "ivanov",
                    Email = "ivanov@training.ru",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                    Role = "Manager",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    FullName = "Петрова Мария",
                    Login = "petrova",
                    Email = "petrova@training.ru",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                    Role = "Applicant",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            };

            context.Users.AddRange(users);
            context.SaveChanges();
        }

        public static ApplicationDbContext CreateAndSeedDictionaryDatabase()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);
            SeedDirectionsAndFormats(context);
            return context;
        }

        public static void SeedDirectionsAndFormats(ApplicationDbContext context)
        {
            // Направления
            var directions = new List<Direction>
            {
                new Direction { Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), Name = "Программирование" },
                new Direction { Id = Guid.Parse("10000000-0000-0000-0000-000000000002"), Name = "Дизайн" },
                new Direction { Id = Guid.Parse("10000000-0000-0000-0000-000000000003"), Name = "Менеджмент" },
                new Direction { Id = Guid.Parse("10000000-0000-0000-0000-000000000004"), Name = "Маркетинг" },
                new Direction { Id = Guid.Parse("10000000-0000-0000-0000-000000000005"), Name = "DevOps" },
                new Direction { Id = Guid.Parse("10000000-0000-0000-0000-000000000006"), Name = "Кибербезопасность" }
            };

            // Форматы
            var formats = new List<TrainingFormat>
            {
                new TrainingFormat { Id = Guid.Parse("20000000-0000-0000-0000-000000000001"), Name = "Очный" },
                new TrainingFormat { Id = Guid.Parse("20000000-0000-0000-0000-000000000002"), Name = "Онлайн" },
                new TrainingFormat { Id = Guid.Parse("20000000-0000-0000-0000-000000000003"), Name = "Вебинар" },
                new TrainingFormat { Id = Guid.Parse("20000000-0000-0000-0000-000000000004"), Name = "Интенсив" }
            };

            // Заявка 
            var application = new Application
            {
                Id = Guid.Parse("a0000000-0000-0000-0000-000000000099"),
                Title = "Тестовая заявка",
                Description = "Тестовое описание",
                Status = "New",
                DirectionId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                FormatId = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                AuthorId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                CreatedAt = DateTime.UtcNow
            };

            context.Directions.AddRange(directions);
            context.TrainingFormats.AddRange(formats);
            context.Applications.Add(application);
            context.SaveChanges();
        }

        // Для пустой базы
        public static ApplicationDbContext CreateEmptyDatabase()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}