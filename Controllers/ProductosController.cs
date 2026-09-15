using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tp5_Trani_Joaco_Alex.Data;
using tp5_Trani_Joaco_Alex.Models;

namespace tp5_Trani_Joaco_Alex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProductos([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var productos = await _context.Productos
                .Where(p => p.Activo)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return Ok(productos);
        }

        [HttpPost]
        public async Task<IActionResult> CrearProducto([FromBody] Productos producto)
        {
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
            return Ok(new { Mensaje = "Producto creado", Producto = producto });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarProducto(int id, [FromBody] Productos producto)
        {
            if (id != producto.ProductoId) return BadRequest("ID incorrecto.");
            _context.Entry(producto).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { Mensaje = "Producto actualizado" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();
            
            producto.Activo = false; // Soft Delete
            await _context.SaveChangesAsync();
            return Ok(new { Mensaje = "Producto eliminado" });
        }
    }
}