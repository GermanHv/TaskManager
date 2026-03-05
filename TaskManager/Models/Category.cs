namespace TaskManager.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; } //codigo corto
        public bool IsActive { get; set; } // para futuros catalogos
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
