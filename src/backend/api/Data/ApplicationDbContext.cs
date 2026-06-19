using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using api.Models.Entities;

namespace api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Direction> Directions { get; set; }
        public DbSet<TrainingFormat> TrainingFormats { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<StatusHistory> StatusHistories { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Уникальные индексы
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Login)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Направления обучения
            modelBuilder.Entity<Direction>().HasData(
                new Direction { Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), Name = "Программирование" },
                new Direction { Id = Guid.Parse("10000000-0000-0000-0000-000000000002"), Name = "Дизайн" },
                new Direction { Id = Guid.Parse("10000000-0000-0000-0000-000000000003"), Name = "Менеджмент" },
                new Direction { Id = Guid.Parse("10000000-0000-0000-0000-000000000004"), Name = "Маркетинг" }
            );

            // Форматы обучения
            modelBuilder.Entity<TrainingFormat>().HasData(
                new TrainingFormat { Id = Guid.Parse("20000000-0000-0000-0000-000000000001"), Name = "Очный" },
                new TrainingFormat { Id = Guid.Parse("20000000-0000-0000-0000-000000000002"), Name = "Онлайн" },
                new TrainingFormat { Id = Guid.Parse("20000000-0000-0000-0000-000000000003"), Name = "Вебинар" },
                new TrainingFormat { Id = Guid.Parse("20000000-0000-0000-0000-000000000004"), Name = "Интенсив" }
            );

            // Пользователи
            modelBuilder.Entity<User>().HasData(
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
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333334"),
                    FullName = "Смирнова Анна",
                    Login = "smirnova",
                    Email = "smirnova@training.ru",
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
                },
                new User
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444445"),
                    FullName = "Сидоров Павел",
                    Login = "sidorov",
                    Email = "sidorov@training.ru",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                    Role = "Applicant",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            modelBuilder.Entity<Application>().HasData(
                new Application
                {
                    Id = Guid.Parse("a0000000-0000-0000-0000-000000000001"),
                    Title = "Курс по C# и .NET",
                    Description = "Хочу освоить C# и ASP.NET Core с нуля. Интересует практический курс с проектами.",
                    Status = "New",
                    DirectionId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                    FormatId = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                    AuthorId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new Application
                {
                    Id = Guid.Parse("a0000000-0000-0000-0000-000000000002"),
                    Title = "DevOps-инжиниринг с нуля",
                    Description = "Планирую освоить CI/CD, Docker и Kubernetes. Нужна программа с практическими лабораторными.",
                    Status = "New",
                    DirectionId = Guid.Parse("10000000-0000-0000-0000-000000000004"),
                    FormatId = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                    AuthorId = Guid.Parse("44444444-4444-4444-4444-444444444445"),
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    AssignedToId = Guid.Parse("22222222-2222-2222-2222-222222222222")
                },
                new Application
                {
                    Id = Guid.Parse("a0000000-0000-0000-0000-000000000003"),
                    Title = "Курс по веб-дизайну и Figma",
                    Description = "Нужно обучение веб-дизайну с упором на Figma и UI/UX. Хочу сменить профессию.",
                    Status = "InProgress",
                    DirectionId = Guid.Parse("10000000-0000-0000-0000-000000000002"),
                    FormatId = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                    AuthorId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                    AssignedToId = Guid.Parse("33333333-3333-3333-3333-333333333333")
                },
                new Application
                {
                    Id = Guid.Parse("a0000000-0000-0000-0000-000000000004"),
                    Title = "Интенсив по Python для анализа данных",
                    Description = "Интересует интенсивный курс по Python с уклоном в数据分析 (Pandas, NumPy, Matplotlib).",
                    Status = "Approved",
                    DirectionId = Guid.Parse("10000000-0000-0000-0000-000000000001"),
                    FormatId = Guid.Parse("20000000-0000-0000-0000-000000000004"),
                    AuthorId = Guid.Parse("33333333-3333-3333-3333-333333333334"),
                    CreatedAt = DateTime.UtcNow.AddDays(-7),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2),
                    AssignedToId = Guid.Parse("22222222-2222-2222-2222-222222222222")
                },
                new Application
                {
                    Id = Guid.Parse("a0000000-0000-0000-0000-000000000005"),
                    Title = "Курс по управлению проектами (PM)",
                    Description = "Хочу изучить основы управления проектами, Agile, Scrum. Для перехода на позицию PM.",
                    Status = "Completed",
                    DirectionId = Guid.Parse("10000000-0000-0000-0000-000000000003"),
                    FormatId = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                    AuthorId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    UpdatedAt = DateTime.UtcNow.AddDays(-25),
                    AssignedToId = Guid.Parse("11111111-1111-1111-1111-111111111111")
                }
            );
        }
    }
}