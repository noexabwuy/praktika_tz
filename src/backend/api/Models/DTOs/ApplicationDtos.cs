using System;
using System.ComponentModel.DataAnnotations;

namespace api.Models.DTOs
{

    public class CreateApplicationRequestDto
    {
        [Required(ErrorMessage = "Название заявки обязательно")]
        [MinLength(3, ErrorMessage = "Название должно содержать минимум 3 символа")]
        [MaxLength(200, ErrorMessage = "Название не должно превышать 200 символов")]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "Описание заявки обязательно")]
        [MinLength(10, ErrorMessage = "Описание должно содержать минимум 10 символов")]
        [MaxLength(2000, ErrorMessage = "Описание не должно превышать 2000 символов")]
        public string Description { get; set; } = "";

        [Required(ErrorMessage = "Направление обучения (directionId) обязательно")]
        public Guid DirectionId { get; set; }

        [Required(ErrorMessage = "Формат обучения (trainingFormatId) обязателен")]
        public Guid FormatId { get; set; }
    }

    public class ApplicationResponseDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Status { get; set; } = "New";
        public Guid DirectionId { get; set; }
        public Guid FormatId { get; set; }
        public Guid AuthorId { get; set; }
        public Guid? AssignedToId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Дополнительные поля для фронтенда (из связанных таблиц)
        public string DirectionName { get; set; } = "";
        public string FormatName { get; set; } = "";
        public string AuthorName { get; set; } = "";
        public string? AssignedToName { get; set; }
    }

    public class AssignApplicationRequestDto
    {
        [Required(ErrorMessage = "Идентификатор ответственного менеджера обязателен.")]
        public Guid AssignedToId { get; set; }
    }

    public class UpdateApplicationStatusRequestDto
    {
        [Required(ErrorMessage = "Статус обязателен для заполнения.")]
        [RegularExpression("^(New|InProgress|NeedsInfo|Approved|Rejected|Completed)$", 
            ErrorMessage = "Недопустимый статус. Допустимые: New, InProgress, NeedsInfo, Approved, Rejected, Completed")]
        public string Status { get; set; } = "";
    }

    // --- DTO ДЛЯ КОММЕНТАРИЕВ ---

    public class CreateCommentRequestDto
    {
        [Required(ErrorMessage = "Текст комментария обязателен для заполнения.")]
        [MinLength(1, ErrorMessage = "Комментарий не может быть пустым.")]
        [MaxLength(1000, ErrorMessage = "Комментарий не должен превышать 1000 символов.")]
        public string Text { get; set; } = "";
    }

    public class CommentResponseDto
    {
        public Guid Id { get; set; }
        public Guid ApplicationId { get; set; }
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; } = "";
        public string Text { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    // --- DTO ДЛЯ АНАЛИТИКИ ---

    public class AnalyticsStatisticsDto
    {
        public int TotalApplications { get; set; }
        public Dictionary<string, int> ByStatuses { get; set; } = new();
        public Dictionary<string, int> ByDirections { get; set; } = new();
    }
}