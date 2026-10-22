using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tp5_Trani_Joaco_Alex.Data;
using tp5_Trani_Joaco_Alex.Models;

namespace tp5_Trani_Joaco_Alex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Bloqueamos el controlador para usuarios no autenticados
    public class ProveedoresController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProveedoresController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProveedores([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var proveedores = await _context.Proveedores
                    .Where(p => p.Activo)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(proveedores);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error recuperando proveedores.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProveedor(int id)
        {
            try
            {
                var proveedor = await _context.Proveedores
                    .FirstOrDefaultAsync(p => p.ProveedorId == id && p.Activo);

                if (proveedor == null) return NotFound(new { mensaje = "Proveedor no encontrado." });

                return Ok(proveedor);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error recuperando el proveedor.", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)] // Solo Admin da de alta proveedores
        public async Task<IActionResult> CrearProveedor([FromBody] Proveedores proveedor)
        {
            if (proveedor == null) return BadRequest(new { mensaje = "El proveedor proporcionado es nulo." });

            try
            {
                proveedor.Activo = true; // Forzamos que nazca activo
                _context.Proveedores.Add(proveedor);
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Proveedor creado exitosamente.", proveedor });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al crear el proveedor.", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> ActualizarProveedor(int id, [FromBody] Proveedores proveedorActualizado)
        {
            if (proveedorActualizado == null || id != proveedorActualizado.ProveedorId)
                return BadRequest(new { mensaje = "Datos inválidos o el ID de la URL no coincide con el del cuerpo." });

            try
            {
                var proveedorExistente = await _context.Proveedores
                    .FirstOrDefaultAsync(p => p.ProveedorId == id && p.Activo);

                if (proveedorExistente == null)
                    return NotFound(new { mensaje = "Proveedor no encontrado o inactivo." });

                // Mapeamos los datos manteniendo intacto Activo = true
                proveedorExistente.RazonSocial = proveedorActualizado.RazonSocial;
                proveedorExistente.Telefono = proveedorActualizado.Telefono;
                proveedorExistente.Email = proveedorActualizado.Email;

                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Proveedor actualizado exitosamente.", proveedor = proveedorExistente });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al actualizar el proveedor.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> EliminarProveedor(int id)
        {
            try
            {
                var proveedor = await _context.Proveedores.FindAsync(id);
                if (proveedor == null) return NotFound(new { mensaje = "Proveedor no encontrado." });

                proveedor.Activo = false; // Soft delete
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Proveedor eliminado lógicamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al eliminar el proveedor.", error = ex.Message });
            }
        }
    }
}