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

        // Listar todas las salidas (Historial) CON PAGINACIÓN
        [HttpGet]
        public async Task<IActionResult> GetSalidas([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var salidas = await _context.SalidaProductos
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(salidas);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error recuperando el historial de salidas.", error = ex.Message });
            }
        }

        // Buscar el detalle de una salida específica
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSalida(int id)
        {
            try
            {
                var salida = await _context.SalidaProductos.FindAsync(id);

                if (salida == null)
                {
                    return NotFound(new { mensaje = "Salida no encontrada." });
                }

                return Ok(salida);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error recuperando la salida.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarSalida([FromBody] SalidaProductos nuevaSalida)
        {
            if (nuevaSalida == null) return BadRequest(new { mensaje = "Los datos de la salida son nulos." });

            try
            {
                var producto = await _context.Productos.FindAsync(nuevaSalida.ProductoId);
                if (producto == null) return NotFound(new { mensaje = "Producto no encontrado." });

                if (producto.Stock < nuevaSalida.Cantidad)
                {
                    return BadRequest(new { mensaje = $"Stock insuficiente. Disponible: {producto.Stock}" });
                }

                producto.Stock -= nuevaSalida.Cantidad;
                nuevaSalida.Fecha = DateTime.Now;

                _context.SalidaProductos.Add(nuevaSalida);
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Venta registrada exitosamente.", StockRestante = producto.Stock, SalidaId = nuevaSalida.SalidaProductoId });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error registrando la salida.", error = ex.Message });
            }
        }

        // Modificar una salida existente
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarSalida(int id, [FromBody] SalidaProductos salidaModificada)
        {
            if (salidaModificada == null || id != salidaModificada.SalidaProductoId)
            {
                return BadRequest(new { mensaje = "Datos inválidos o el ID de la URL no coincide con el del registro." });
            }

            try
            {
                if (!SalidaExiste(id))
                {
                    return NotFound(new { mensaje = "Salida no encontrada para actualizar." });
                }

                _context.Entry(salidaModificada).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Salida actualizada exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al actualizar la salida.", error = ex.Message });
            }
        }

        // Eliminar un registro de salida
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarSalida(int id)
        {
            try
            {
                var salida = await _context.SalidaProductos.FindAsync(id);
                if (salida == null)
                {
                    return NotFound(new { mensaje = "Salida no encontrada para eliminar." });
                }

                _context.SalidaProductos.Remove(salida);
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Salida eliminada exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error eliminando la salida.", error = ex.Message });
            }
        }

        // Método auxiliar necesario para el PUT
        private bool SalidaExiste(int id)
        {
            return _context.SalidaProductos.Any(e => e.SalidaProductoId == id);
        }
    }
}