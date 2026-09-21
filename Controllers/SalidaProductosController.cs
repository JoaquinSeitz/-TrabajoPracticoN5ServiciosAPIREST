using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tp5_Trani_Joaco_Alex.Data;
using tp5_Trani_Joaco_Alex.Models;

namespace tp5_Trani_Joaco_Alex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalidaProductosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SalidaProductosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarSalida([FromBody] SalidaProductos nuevaSalida)
        {
            var producto = await _context.Productos.FindAsync(nuevaSalida.ProductoId);
            if (producto == null) return NotFound("Producto no encontrado.");

            if (producto.Stock < nuevaSalida.Cantidad)
            {
                return BadRequest($"Stock insuficiente. Disponible: {producto.Stock}");
            }

            producto.Stock -= nuevaSalida.Cantidad;
            nuevaSalida.Fecha = DateTime.Now;

            _context.SalidaProductos.Add(nuevaSalida);
            await _context.SaveChangesAsync();

            return Ok(new { Mensaje = "Venta registrada", StockRestante = producto.Stock });
        }
    }
}