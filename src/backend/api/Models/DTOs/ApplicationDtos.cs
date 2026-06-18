using System;
using System.ComponentModel.DataAnnotations;

namespace api.Models.DTOs
{
    public class CreateApplicationRequestDto
    {
        [Required(ErrorMessage = "Направление обучения (directionId) обязательно для заполнения.")]
        public Guid? DirectionId { get; set; }

        [Required(ErrorMessage = "Формат обучения (trainingFormatId) обязателен для заполнения.")]
        public Guid? TrainingFormatId { get; set; }

        [Required(ErrorMessage = "Описание заявки обязательно для заполнения.")]
        [MinLength(10, ErrorMessage = "Описание должно содержать минимум 10 символов.")]
        [MaxLength(2000, ErrorMessage = "Описание не должно превышать 2000 символов.")]
        public string Description { get; set; } = "";
    }

    public class ApplicationResponseDto
    {
        public Guid Id { get; set; }
        public Guid DirectionId { get; set; }
        public Guid TrainingFormatId { get; set; }
        public string Status { get; set; } = "New";
        public DateTime CreatedAt { get; set; }
    }

    public class AssignApplicationRequestDto
    {
        [Required(ErrorMessage = "Идентификатор ответственного менеджера обязателен.")]
        public Guid AssignedToId { get; set; }
    }

    public class UpdateApplicationStatusRequestDto
    {
        [Required(ErrorMessage = "Статус обязателен для заполнения.")]
        [RegularExpression("^(New|InProgress|ClarificationRequired|Approved|Rejected)$", 
            ErrorMessage = "Недопустимый статус. Допустимые: New, InProgress, ClarificationRequired, Approved, Rejected")]
        public string Status { get; set; } = "";
    }
}