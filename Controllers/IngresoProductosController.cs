using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tp5_Trani_Joaco_Alex.Data;
using tp5_Trani_Joaco_Alex.Models;
using tp5_Trani_Joaco_Alex.DTOs.Request;
using tp5_Trani_Joaco_Alex.DTOs.Response;

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

        [HttpGet]
        public async Task<IActionResult> GetIngresos([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var ingresos = await _context.IngresoProductos
                    .Include(i => i.Producto)
                    .Include(i => i.Proveedor)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(i => new IngresoProductoResponseDTO
                    {
                        IngresoProductoId = i.IngresoProductoId,
                        Fecha = i.Fecha,
                        Cantidad = i.Cantidad,
                        ProductoId = i.ProductoId,
                        ProductoNombre = i.Producto != null ? i.Producto.Nombre : "Desconocido",
                        ProveedorId = i.ProveedorId,
                        ProveedorNombre = i.Proveedor != null ? i.Proveedor.RazonSocial : "Desconocido",
                        UsuarioId = i.UsuarioId
                    })
                    .ToListAsync();

                return Ok(ingresos);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error recuperando el historial de ingresos.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetIngreso(int id)
        {
            try
            {
                var ingreso = await _context.IngresoProductos
                    .Include(i => i.Producto)
                    .Include(i => i.Proveedor)
                    .FirstOrDefaultAsync(i => i.IngresoProductoId == id);

                if (ingreso == null) return NotFound(new { mensaje = "Ingreso no encontrado." });

                var response = new IngresoProductoResponseDTO
                {
                    IngresoProductoId = ingreso.IngresoProductoId,
                    Fecha = ingreso.Fecha,
                    Cantidad = ingreso.Cantidad,
                    ProductoId = ingreso.ProductoId,
                    ProductoNombre = ingreso.Producto != null ? ingreso.Producto.Nombre : "Desconocido",
                    ProveedorId = ingreso.ProveedorId,
                    ProveedorNombre = ingreso.Proveedor != null ? ingreso.Proveedor.RazonSocial : "Desconocido",
                    UsuarioId = ingreso.UsuarioId
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error recuperando el ingreso.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarIngreso([FromBody] IngresoProductoRequestDTO dto)
        {
            if (dto == null) return BadRequest(new { mensaje = "Los datos del ingreso son nulos." });

            try
            {
                var producto = await _context.Productos.FindAsync(dto.ProductoId);
                if (producto == null) return NotFound(new { mensaje = "Producto no encontrado." });

                // Sumamos el stock al producto
                producto.Stock += dto.Cantidad;

                // Creamos la entidad segura
                var nuevoIngreso = new IngresoProductos
                {
                    Cantidad = dto.Cantidad,
                    ProductoId = dto.ProductoId,
                    ProveedorId = dto.ProveedorId,
                    UsuarioId = dto.UsuarioId,
                    Fecha = DateTime.Now // El servidor dicta la fecha, no el cliente
                };

                _context.IngresoProductos.Add(nuevoIngreso);
                await _context.SaveChangesAsync();

                return Ok(new { Mensaje = "Ingreso registrado", StockActual = producto.Stock, IngresoId = nuevoIngreso.IngresoProductoId });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error registrando el ingreso.", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarIngreso(int id, [FromBody] IngresoProductoRequestDTO dto)
        {
            if (dto == null) return BadRequest(new { mensaje = "Los datos del ingreso son nulos." });

            try
            {
                var ingresoExistente = await _context.IngresoProductos.FindAsync(id);
                if (ingresoExistente == null) return NotFound(new { mensaje = "Ingreso no encontrado para actualizar." });

                // Ojo: en un sistema real de inventario, modificar la cantidad en un PUT 
                // implicaría recalcular el stock del Producto (restar la cantidad vieja y sumar la nueva). 
                // Aquí solo actualizamos los datos básicos del registro por seguridad del DTO.
                ingresoExistente.Cantidad = dto.Cantidad;
                ingresoExistente.ProductoId = dto.ProductoId;
                ingresoExistente.ProveedorId = dto.ProveedorId;
                ingresoExistente.UsuarioId = dto.UsuarioId;

                await _context.SaveChangesAsync();
                return Ok(new { mensaje = "Ingreso actualizado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error actualizando el ingreso.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarIngreso(int id)
        {
            try
            {
                var ingreso = await _context.IngresoProductos.FindAsync(id);
                if (ingreso == null) return NotFound(new { mensaje = "Ingreso no encontrado para eliminar." });

                _context.IngresoProductos.Remove(ingreso);
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Ingreso eliminado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error eliminando el ingreso.", error = ex.Message });
            }
        }
    }
}