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
        public async Task<IActionResult> GetCategorias([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var categorias = await _context.Categorias
                .Where(c => c.Activo)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return Ok(categorias);
        }

        [HttpPost]
        public async Task<IActionResult> CrearCategoria([FromBody] Categoria categoria)
        {
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();
            return Ok(categoria);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarCategoria(int id, [FromBody] Categoria categoria)
        {
            if (id != categoria.CategoriaId) return BadRequest();
            _context.Entry(categoria).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return NotFound();

            categoria.Activo = false;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}