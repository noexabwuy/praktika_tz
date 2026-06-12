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

        public DbSet<AppUser> AppUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ServiceType>().HasData(
                new ServiceType { Id = 1, Name = "��������", Description = "����� � ��������" },
                new ServiceType { Id = 2, Name = "������������", Description = "�������������� ������������" },
                new ServiceType { Id = 3, Name = "�����������", Description = "��������, ��������" }
            );

            modelBuilder.Entity<ApplicationStatus>().HasData(
                new ApplicationStatus { Id = 1, Name = "�����", Color = "#007bff", SortOrder = 1 },
                new ApplicationStatus { Id = 2, Name = "� ������", Color = "#ffc107", SortOrder = 2 },
                new ApplicationStatus { Id = 3, Name = "��������� ���������", Color = "#6c757d", SortOrder = 3 },
                new ApplicationStatus { Id = 4, Name = "�����������", Color = "#28a745", SortOrder = 4 },
                new ApplicationStatus { Id = 5, Name = "���������", Color = "#dc3545", SortOrder = 5 },
                new ApplicationStatus { Id = 6, Name = "���������", Color = "#6c757d", SortOrder = 6 }
            );

            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, FullName = "�������������", Username = "admin", Password = "admin123", Role = "Admin", IsActive = true },
                new User { Id = 2, FullName = "������� �����", Username = "manager", Password = "mary123", Role = "Manager", IsActive = true },
                new User { Id = 3, FullName = "������� �������", Username = "alexey18", Password = "alex123", Role = "Manager", IsActive = true }
            );
        }
    }
}