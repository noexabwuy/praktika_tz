using System.ComponentModel.DataAnnotations;

namespace api.Models.DTOs
{
    public class DictionaryRequestDto
    {
        [Required(ErrorMessage = "Название обязательно")]
        [MinLength(2, ErrorMessage = "Название должно содержать минимум 2 символа")]
        [MaxLength(100, ErrorMessage = "Название не должно превышать 100 символов")]
        public string Name { get; set; } = "";
    }
}