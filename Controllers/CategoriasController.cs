using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tp5_Trani_Joaco_Alex.Data;
using tp5_Trani_Joaco_Alex.Models;

namespace tp5_Trani_Joaco_Alex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous] // Público para poder filtrar el catálogo en la App Móvil y Web
        public async Task<IActionResult> GetCategorias([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var categorias = await _context.Categorias
                    .Where(c => c.Activo)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(categorias);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error recuperando categorías.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategoria(int id)
        {
            try
            {
                var categoria = await _context.Categorias
                    .FirstOrDefaultAsync(c => c.CategoriaId == id && c.Activo);

                if (categoria == null) return NotFound(new { mensaje = "Categoría no encontrada." });

                return Ok(categoria);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error recuperando la categoría.", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> CrearCategoria([FromBody] Categoria categoria)
        {
            if (categoria == null) return BadRequest(new { mensaje = "La categoría proporcionada es nula." });

            try
            {
                categoria.Activo = true; // Nos aseguramos de que nazca activa
                _context.Categorias.Add(categoria);
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Categoría creada exitosamente.", categoria });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error creando categoría.", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> ActualizarCategoria(int id, [FromBody] Categoria categoriaActualizada)
        {
            if (categoriaActualizada == null || id != categoriaActualizada.CategoriaId)
                return BadRequest(new { mensaje = "Datos inválidos o el ID de la URL no coincide con el del cuerpo." });

            try
            {
                var categoriaExistente = await _context.Categorias
                    .FirstOrDefaultAsync(c => c.CategoriaId == id && c.Activo);

                if (categoriaExistente == null)
                    return NotFound(new { mensaje = "Categoría no encontrada o inactiva." });

                // Actualizamos solo el nombre sin poner en riesgo el estado Activo
                categoriaExistente.Nombre = categoriaActualizada.Nombre;
                categoriaExistente.Descripcion = categoriaActualizada.Descripcion;

                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Categoría actualizada exitosamente.", categoria = categoriaExistente });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error actualizando categoría.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> EliminarCategoria(int id)
        {
            try
            {
                var categoria = await _context.Categorias.FindAsync(id);
                if (categoria == null) return NotFound(new { mensaje = "Categoría no encontrada." });

                categoria.Activo = false; // Soft Delete
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Categoría eliminada lógicamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error eliminando categoría.", error = ex.Message });
            }
        }
    }
}