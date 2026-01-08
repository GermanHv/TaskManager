namespace TaskManager.DTOs
{
    public class TaskQueryResultDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsCompleted { get; set; }
        public int Step { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
