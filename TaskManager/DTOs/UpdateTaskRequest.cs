using System.ComponentModel.DataAnnotations;

namespace TaskManager.DTOs
{
    public class UpdateTaskRequest
    {
        [Required(ErrorMessage = "El título es obligatorio.")]
        [MaxLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
        public string Title { get; set; }
        public bool? IsCompleted { get; set; }
    }
}