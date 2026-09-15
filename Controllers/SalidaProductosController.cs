using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
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
            // 1. Buscamos el producto
            var producto = await _context.Productos.FindAsync(nuevaSalida.ProductoId);
            if (producto == null) return NotFound("Producto no encontrado.");

            // 2. VALIDACIÓN ESTRICTA
            if (producto.Stock < nuevaSalida.Cantidad)
            {
                return BadRequest($"Operación rechazada: Stock insuficiente. Stock disponible: {producto.Stock}");
            }

            // 3. Restamos el stock
            producto.Stock -= nuevaSalida.Cantidad;

            // 4. Asignamos la fecha actual
            nuevaSalida.Fecha = DateTime.Now;

            // 5. Guardamos en la base de datos
            _context.SalidaProductos.Add(nuevaSalida);
            await _context.SaveChangesAsync();

            return Ok(new { Mensaje = "Venta registrada con éxito", StockRestante = producto.Stock });
        }
    }
}