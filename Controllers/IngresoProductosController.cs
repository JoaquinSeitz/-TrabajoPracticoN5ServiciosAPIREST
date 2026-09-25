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


        //  Listar todos los ingresos (Historial)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IngresoProductos>>> GetIngresos()
        {
            return await _context.IngresoProductos.ToListAsync();
        }

        // Buscar el detalle de un ingreso específico
        [HttpGet("{id}")]
        public async Task<ActionResult<IngresoProductos>> GetIngreso(int id)
        {
            var ingreso = await _context.IngresoProductos.FindAsync(id);

            if (ingreso == null)
            {
                return NotFound("Ingreso no encontrado.");
            }

            return ingreso;
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
    
       // Modificar un ingreso existente
        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarIngreso(int id, [FromBody] IngresoProductos ingresoModificado)
        {
            if (id != ingresoModificado.IngresoProductoId)
            {
                return BadRequest("El ID de la URL no coincide con el del registro.");
            }

            _context.Entry(ingresoModificado).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IngresoExiste(id))
                {
                    return NotFound("Ingreso no encontrado para actualizar.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // 204 No Content indica que se actualizó con éxito
        }

        // Eliminar un registro de ingreso
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarIngreso(int id)
        {
            var ingreso = await _context.IngresoProductos.FindAsync(id);
            if (ingreso == null)
            {
                return NotFound("Ingreso no encontrado para eliminar.");
            }

            _context.IngresoProductos.Remove(ingreso);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Método auxiliar necesario para el PUT
        private bool IngresoExiste(int id)
        {
            return _context.IngresoProductos.Any(e => e.IngresoProductoId == id);
        }
    }
}
