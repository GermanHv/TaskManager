namespace TaskManager.DTOs.TaskDto
{
    public class SearchFilter : PaginationDto
    {
        public string? text { get; set; }
        public bool? completed { get; set; }
        public int? step { get; set; }
        public string? orderBy { get; set; }
    }
}
