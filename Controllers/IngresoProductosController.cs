using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tp5_Trani_Joaco_Alex.Data;
using tp5_Trani_Joaco_Alex.Models;

namespace tp5_Trani_Joaco_Alex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IngresoProductosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public IngresoProductosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarIngreso([FromBody] IngresoProductos nuevoIngreso)
        {
            var producto = await _context.Productos.FindAsync(nuevoIngreso.ProductoId);
            if (producto == null) return NotFound("Producto no encontrado.");

            producto.Stock += nuevoIngreso.Cantidad;
            nuevoIngreso.Fecha = DateTime.Now;

            _context.IngresoProductos.Add(nuevoIngreso);
            await _context.SaveChangesAsync();

            return Ok(new { Mensaje = "Ingreso registrado", StockActual = producto.Stock });
        }
    }
}