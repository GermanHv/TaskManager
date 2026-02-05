using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Context;
using TaskManager.DTOs.CategoryDto;
using TaskManager.Models;

namespace TaskManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> Get()
        {
            var result = await _context.Categories
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();

            return Ok(result);
        }

        // Post
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryRequest request)
        {
            // Si usas [ApiController], ModelState se valida automáticamente.
            // Igual puedes explicar que si falla devuelve 400.

            var entity = new Category
            {
                Name = request.Name.Trim()
            };

            _context.Categories.Add(entity);
            await _context.SaveChangesAsync();

            var dto = new CategoryDto
            {
                Id = entity.Id,
                Name = entity.Name
            };

            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }

        // obtener categoría en especifico
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoryDto>> GetById(int id)
        {
            var entity = await _context.Categories.FindAsync(id);
            if (entity == null) return NotFound();

            var dto = new CategoryDto
            {
                Id = entity.Id,
                Name = entity.Name
            };

            return Ok(dto);
        }

        [HttpPost("import-excel")] // Para enviar/cargar archivos se envian por metodo POST (creo se puede con PUT) 
        public async Task<IActionResult> ImportFromExcel(IFormFile file) // Se recibe el excel
        {
            // Validación del archivo **se debe de migrar la lógica al servicio
            if (file == null || file.Length == 0)
                return BadRequest("No se recibió ningún archivo o está vacío.");

            var categories = new List<Category>();

            using (var stream = new MemoryStream()) // Crea un archivo virtual en la ram 
            {
                await file.CopyToAsync(stream); // copia el archivo que estoy recibiendo en el archivo virtual
                stream.Position = 0; // Nos aseguramos de ir al inicio

                using (var workbook = new XLWorkbook(stream)) // abre el excel que tenemos de copia en memoria ram
                {
                    var worksheet = workbook.Worksheets.First(); // Tomamos la primera hoja
                    var rows = worksheet.RangeUsed().RowsUsed(); // Contamos cuantas columnas estan en uso

                    bool isHeader = true; 

                    foreach (var row in rows)
                    {
                        // Lógica para saltar la cabecera
                        if (isHeader)
                        {
                            isHeader = false;
                            continue;
                        }

                        // Extracción de datos por columnas
                        var name = row.Cell(1).GetString();       // Columna A
                        var code = row.Cell(2).GetString();       // Columna B
                        var isActiveCell = row.Cell(3).GetString(); // Columna C

                        // Lógica de conversión para el booleano
                        bool isActive = true;
                        if (!string.IsNullOrWhiteSpace(isActiveCell))
                        {
                            // TRUE/FALSE, 1/0, Sí/No... aquí se puede refinar
                            bool.TryParse(isActiveCell, out isActive);
                        }

                        // Validación de nombre obligatorio
                        if (string.IsNullOrWhiteSpace(name))
                        {
                            // Se puede decidir saltar o romper
                            continue;
                        }

                        // Creación del objeto
                        var category = new Category
                        {
                            Name = name.Trim(),
                            Code = code?.Trim(),
                            IsActive = isActive
                        };

                        categories.Add(category);
                    }
                }
            }
            // Validaciones
            // A. Opcional: filtrar duplicados por Name en la misma importación (Excel)
            // Esto evita que si el excel trae dos veces "Bebidas", se intente insertar dos veces.
            categories = categories
                .GroupBy(c => c.Name.ToLower())
                .Select(g => g.First())
                .ToList();

            // B. Opcional: evitar insertar categorías que ya existan en la BD
            // Traemos solo los nombres existentes para comparar en memoria (más eficiente que consultar uno por uno)
            var existingNames = _context.Categories
                .Select(c => c.Name.ToLower())
                .ToHashSet(); // HashSet es muy rápido para búsquedas

            // Filtramos la lista 'categories' dejando solo las que NO están en 'existingNames'
            var newCategories = categories
                .Where(c => !existingNames.Contains(c.Name.ToLower()))
                .ToList();

            // 8. Guardar solo las nuevas categorías
            // Nota: Usamos 'newCategories' en lugar de 'categories' para aplicar el filtro
            if (newCategories.Any())
            {
                _context.Categories.AddRange(newCategories);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                Message = $"Se importaron {newCategories.Count} categorías."
            });
        }

    }
}
