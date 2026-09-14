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
            // 1. Buscamos el producto
            var producto = await _context.Productos.FindAsync(nuevoIngreso.ProductoId);
            if (producto == null) return NotFound("Producto no encontrado.");

            // 2. Sumamos el stock
            producto.Stock += nuevoIngreso.Cantidad;

            // 3. Asignamos la fecha actual
            nuevoIngreso.Fecha = DateTime.Now;

            // 4. Guardamos el registro y confirmamos
            _context.IngresoProductos.Add(nuevoIngreso);
            await _context.SaveChangesAsync();

            return Ok(new { Mensaje = "Ingreso registrado con éxito", StockActual = producto.Stock });
        }
    }
}