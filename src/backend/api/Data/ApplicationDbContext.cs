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
                new User { Id = 1, FullName = "Администратор", Username = "admin", Password = "admin123", Role = "Admin", IsActive = true },
                new User { Id = 2, FullName = "Петрова Мария", Username = "manager", Password = "mary123", Role = "Manager", IsActive = true },
                new User { Id = 3, FullName = "Сидоров Алексей", Username = "alexey18", Password = "alex123", Role = "Manager", IsActive = true }
            );
        }
    }
}