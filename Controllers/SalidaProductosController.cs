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


        //  Listar todas las salidas (Historial)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SalidaProductos>>> GetSalidas()
        {
            return await _context.SalidaProductos.ToListAsync();
        }

        //  GET {id}: Buscar el detalle de una salida específica
        [HttpGet("{id}")]
        public async Task<ActionResult<SalidaProductos>> GetSalida(int id)
        {
            var salida = await _context.SalidaProductos.FindAsync(id);

            if (salida == null)
            {
                return NotFound("Salida no encontrada.");
            }

            return salida;
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
    

    //  Modificar una salida existente
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarSalida(int id, [FromBody] SalidaProductos salidaModificada)
        {
            if (id != salidaModificada.SalidaProductoId)
            {
                return BadRequest("El ID de la URL no coincide con el del registro.");
            }

            _context.Entry(salidaModificada).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SalidaExiste(id))
                {
                    return NotFound("Salida no encontrada para actualizar.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        //  DELETE: Eliminar un registro de salida
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarSalida(int id)
        {
            var salida = await _context.SalidaProductos.FindAsync(id);
            if (salida == null)
            {
                return NotFound("Salida no encontrada para eliminar.");
            }

            _context.SalidaProductos.Remove(salida);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Método auxiliar necesario para el PUT
        private bool SalidaExiste(int id)
        {
            return _context.SalidaProductos.Any(e => e.SalidaProductoId == id);
        }
    }
}


