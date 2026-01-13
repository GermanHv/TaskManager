using System.ComponentModel.DataAnnotations;

namespace TaskManager.DTOs.TaskDto
{
    public class CreateTaskRequest
    {
        public string Title {  get; set; }

        [Required(ErrorMessage = "CategoryId es requerido.")]
        public int CategoryId { get; set; }
    }
}
