using TaskManager.DTOs.TaskDto;

namespace TaskManager.Interfaces.Tasks
{
    public interface ITaskService
    {
        Task<PagedResultDto<TaskWithCategoryDto>> AdvancedSearchAsync(
            string? text,
            bool? completed,
            int? step,
            int? categoryId,
            string? categoryName,
            int page,
            int pageSize
         );
    }
}
