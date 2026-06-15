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
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    FullName = "Иванов Иван",
                    Login = "ivanov",
                    Email = "ivanov@training.ru",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                    Role = "Manager",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    FullName = "Петрова Мария",
                    Login = "petrova",
                    Email = "petrova@training.ru",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123"),
                    Role = "Applicant",
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
}