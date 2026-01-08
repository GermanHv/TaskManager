using Microsoft.EntityFrameworkCore.Migrations;
using TaskManager.Migrations;

namespace TaskManager.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsCompleted
        {
            get; set;
        }
        public int Step { get; set; }
        // Guarda los datos de auditoría
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? CategoryId { get; set; } = 0;
        public Category Category { get; set; }
    }
}
