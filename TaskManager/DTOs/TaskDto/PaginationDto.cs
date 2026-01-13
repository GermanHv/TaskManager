using System.ComponentModel.DataAnnotations;

namespace TaskManager.DTOs.TaskDto
{
    public class PaginationDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "La página debe ser mayor a 0")] 
        public int Page { get; set; } = 1;

        // El valor por defecto es 10, y forzamos a que pidan entre 1 y 50 registros máximo.
        [Range(1, 50, ErrorMessage = "El tamaño de página debe estar entre 1 y 50")]
        public int PageSize { get; set; } = 10;
    }
}
