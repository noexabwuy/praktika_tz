using Microsoft.EntityFrameworkCore;
using api.Models.Entities;

namespace api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<ServiceType> ServiceTypes { get; set; }
        public DbSet<ApplicationStatus> ApplicationStatuses { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ServiceType>().HasData(
                new ServiceType { Id = 1, Name = "Обучение", Description = "Курсы и тренинги" },
                new ServiceType { Id = 2, Name = "Консультация", Description = "Индивидуальные консультации" },
                new ServiceType { Id = 3, Name = "Мероприятие", Description = "Вебинары, семинары" }
            );

            modelBuilder.Entity<ApplicationStatus>().HasData(
                new ApplicationStatus { Id = 1, Name = "Новая", Color = "#007bff", SortOrder = 1 },
                new ApplicationStatus { Id = 2, Name = "В работе", Color = "#ffc107", SortOrder = 2 },
                new ApplicationStatus { Id = 3, Name = "Требуется уточнение", Color = "#6c757d", SortOrder = 3 },
                new ApplicationStatus { Id = 4, Name = "Согласована", Color = "#28a745", SortOrder = 4 },
                new ApplicationStatus { Id = 5, Name = "Отклонена", Color = "#dc3545", SortOrder = 5 },
                new ApplicationStatus { Id = 6, Name = "Завершена", Color = "#6c757d", SortOrder = 6 }
            );

            modelBuilder.Entity<User>().HasData(
                new User {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    FullName = "Администратор",
                    Login = "admin",
                    Email = "admin@training.ru",
                    PasswordHash = "admin123",  // должен быть хэш!
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    FullName = "Иванов Иван",
                    Login = "ivanov",
                    Email = "ivanov@training.ru",
                    PasswordHash = "123",
                    Role = "Manager",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    FullName = "Петрова Мария",
                    Login = "petrova",
                    Email = "petrova@training.ru",
                    PasswordHash = "123",
                    Role = "Manager",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    FullName = "Сидоров Алексей",
                    Login = "sidorov",
                    Email = "sidorov@training.ru",
                    PasswordHash = "123",
                    Role = "Applicant",
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
}