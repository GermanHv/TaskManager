using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Context;
using TaskManager.DTOs.TaskDto;
using TaskManager.Interfaces.Tasks;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITaskService _taskService;
        public TasksController(AppDbContext context, ITaskService taskService)
        {
            _context = context;
            _taskService = taskService;
        }
        [HttpGet]
        public async Task<ActionResult< List<TaskItem>>> Get()
        {
                    var tasks = await _context.Tasks
            .Select(t => new TaskItemResponse
            {
                Id = t.Id,
                Title = t.Title,
                IsCompleted = t.IsCompleted
            })
            .ToListAsync();

                    return Ok(tasks);
        }
        // Retorna un registro
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TaskItem>> GetById(int id)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
                return NotFound();

            var dto = new TaskItemResponse
            {
                Id = task.Id,
                Title = task.Title,
                IsCompleted = task.IsCompleted
            };

            return Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<TaskItem>> Create([FromBody] CreateTaskRequest request)
        {
            if (request == null)
                return BadRequest("Body requerido.");

            if (string.IsNullOrWhiteSpace(request.Title))
                return BadRequest("Title es requerido.");

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
            if (!categoryExists)
                return BadRequest("CategoryId no existe.");

            var entity = new TaskItem
            {
                Title = request.Title.Trim(),
                IsCompleted = false,
                CategoryId = request.CategoryId
            };

            // registro
            _context.Tasks.Add(entity);
            int ContadorCambios = await _context.SaveChangesAsync(); 

            var dto = new TaskItemResponse
            {
                Id = entity.Id,
                Title = entity.Title,
                IsCompleted = entity.IsCompleted
            };

            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }

        // Update
        // Delete: no se borra, solo se pone una bandera como True y solo me regresa los que tengan ese registro.
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskRequest request)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            if (request == null) return BadRequest("Body requerido.");
            if (string.IsNullOrWhiteSpace(request.Title)) return BadRequest("Title es requerido.");

            task.Title = request.Title.Trim();
            if (request.IsCompleted.HasValue) {
                task.IsCompleted = request.IsCompleted.Value;
                    }

            await _context.SaveChangesAsync();

            return NoContent(); // 204
        }

        // Delete
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Search
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<TaskQueryResultDto>>> Search(
            [FromQuery] SearchFilter request
            )
        {
            var query = _context.Tasks.AsQueryable();

            // Filtros
            if (!string.IsNullOrWhiteSpace(request.text))
                query = query.Where(t => t.Title.Contains(request.text));

            if (request.completed.HasValue)
                query = query.Where(t => t.IsCompleted == request.completed);

            if (request.step.HasValue)
                query = query.Where(t => t.Step == request.step);

            // Ordenamiento
            query = request.orderBy switch
            {
                "title" => query.OrderBy(t => t.Title),
                "title_desc" => query.OrderByDescending(t => t.Title),
                "date" => query.OrderBy(t => t.CreatedAt),
                "date_desc" => query.OrderByDescending(t => t.CreatedAt),
                "step" => query.OrderBy(t => t.Step),
                "step_desc" => query.OrderByDescending(t => t.Step),
                _ => query.OrderBy(t => t.Id)
            };

            // Paginación
            query = query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize);

            var results = await query
                .Select(t => new TaskQueryResultDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    IsCompleted = t.IsCompleted,
                    Step = t.Step,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return Ok(results);
        }

        [HttpGet("paged")] 
        public async Task<ActionResult<IEnumerable<TaskQueryResultDto>>> GetPaged(
            [FromQuery] PaginationDto pagination // Dto
            ) 
        { 
            var query = _context.Tasks
                .OrderBy(t => t.Id)
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize);

            var result = await query.Select(t => new TaskQueryResultDto 
            { 
                Id = t.Id,
                Title = t.Title,
                IsCompleted = t.IsCompleted,
                Step = t.Step,
                CreatedAt = t.CreatedAt
            })
                .ToListAsync();
            return Ok(result); }

        [HttpGet("with-category")]
        public async Task<ActionResult<IEnumerable<TaskWithCategoryDto>>> GetWithCategory()
        {
            var result = await _context.Tasks
                .Include(t => t.Category) // forma estandard de un JOIN
                .OrderBy(t => t.Id)
                .Select(t => new TaskWithCategoryDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    IsCompleted = t.IsCompleted,
                    Step = t.Step,
                    CreatedAt = t.CreatedAt,
                    CategoryId = t.CategoryId ?? 0,
                    CategoryName = t.Category.Name
                })
                .ToListAsync();

            return Ok(result);
        }


        // Busqueda avanzada
        [HttpGet("advanced-search")]
        public async Task<ActionResult<PagedResultDto<TaskWithCategoryDto>>> AdvancedSearch(
    [FromQuery] string? text,
    [FromQuery] bool? completed,
    [FromQuery] int? step,
    [FromQuery] int? categoryId,
    [FromQuery] string? categoryName,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10
)
        {

            if (page <= 0) return BadRequest("Page debe ser mayor a 0.");
            if (pageSize <= 0 || pageSize > 100) return BadRequest("PageSize debe estar entre 1 y 100.");

            var result = await _taskService.AdvancedSearchAsync(
                text, completed, step, categoryId, categoryName, page, pageSize);

            return Ok(result);
        }

        [HttpGet("ajax-search")]
        public async Task<IActionResult> AjaxSearch([FromQuery] string? text)
        {
            var query = _context.Tasks.AsQueryable();

            if (!string.IsNullOrWhiteSpace(text))
                query = query.Where(t => t.Title.Contains(text));

            var results = await query
                .OrderBy(t => t.Id)
                .Take(50)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    //t.CategoryName,
                    t.IsCompleted,
                    t.Step
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}

