using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = Roles.Admin)]
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
                    .Include(i => i.Proveedor)
                    .Include(i => i.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(i => new IngresoProductoResponseDTO
                    {
                        IngresoProductoId = i.IngresoProductoId,
                        Fecha = i.Fecha,
                        ProveedorId = i.ProveedorId,
                        ProveedorNombre = i.Proveedor != null ? i.Proveedor.RazonSocial : "Desconocido",
                        UsuarioId = i.UsuarioId,
                        Detalles = i.Detalles.Select(d => new DetalleMovimientoResponseDTO
                        {
                            ProductoId = d.ProductoId,
                            ProductoNombre = d.Producto != null ? d.Producto.Nombre : "Desconocido",
                            Cantidad = d.Cantidad
                        }).ToList()
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
                    .Include(i => i.Proveedor)
                    .Include(i => i.Detalles)
                        .ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(i => i.IngresoProductoId == id);

                if (ingreso == null) return NotFound(new { mensaje = "Ingreso no encontrado." });

                var response = new IngresoProductoResponseDTO
                {
                    IngresoProductoId = ingreso.IngresoProductoId,
                    Fecha = ingreso.Fecha,
                    ProveedorId = ingreso.ProveedorId,
                    ProveedorNombre = ingreso.Proveedor != null ? ingreso.Proveedor.RazonSocial : "Desconocido",
                    UsuarioId = ingreso.UsuarioId,
                    Detalles = ingreso.Detalles.Select(d => new DetalleMovimientoResponseDTO
                    {
                        ProductoId = d.ProductoId,
                        ProductoNombre = d.Producto != null ? d.Producto.Nombre : "Desconocido",
                        Cantidad = d.Cantidad
                    }).ToList()
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
            if (dto == null || dto.Detalles == null || !dto.Detalles.Any())
            {
                return BadRequest(new { mensaje = "El ingreso debe contener al menos un producto en el detalle." });
            }

            try
            {
                var nuevoIngreso = new IngresoProductos
                {
                    ProveedorId = dto.ProveedorId,
                    UsuarioId = dto.UsuarioId,
                    Fecha = DateTime.Now,
                    Detalles = new List<DetalleIngresoProductos>()
                };

                foreach (var item in dto.Detalles)
                {
                    if (item.Cantidad <= 0)
                        return BadRequest(new { mensaje = $"La cantidad para el producto ID {item.ProductoId} debe ser mayor a 0." });

                    var producto = await _context.Productos.FindAsync(item.ProductoId);
                    if (producto == null)
                        return NotFound(new { mensaje = $"Producto con ID {item.ProductoId} no encontrado." });

                    // Sumamos el stock al producto
                    producto.Stock += item.Cantidad;

                    nuevoIngreso.Detalles.Add(new DetalleIngresoProductos
                    {
                        ProductoId = item.ProductoId,
                        Cantidad = item.Cantidad
                    });
                }

                _context.IngresoProductos.Add(nuevoIngreso);
                await _context.SaveChangesAsync(); // Guarda cabecera, detalles y actualiza stock en una sola transacción

                return Ok(new { mensaje = "Ingreso registrado exitosamente.", IngresoId = nuevoIngreso.IngresoProductoId });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error registrando el ingreso.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin)] // Solo el Admin puede anular un ingreso
        public async Task<IActionResult> EliminarIngreso(int id)
        {
            try
            {
                var ingreso = await _context.IngresoProductos
                    .Include(i => i.Detalles)
                    .FirstOrDefaultAsync(i => i.IngresoProductoId == id);

                if (ingreso == null) return NotFound(new { mensaje = "Ingreso no encontrado para eliminar." });

                // Revertimos el stock de los productos involucrados antes de borrar el registro
                foreach (var detalle in ingreso.Detalles)
                {
                    var producto = await _context.Productos.FindAsync(detalle.ProductoId);
                    if (producto != null)
                    {
                        producto.Stock -= detalle.Cantidad;
                    }
                }

                _context.IngresoProductos.Remove(ingreso);
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Ingreso eliminado y stock revertido exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error eliminando el ingreso.", error = ex.Message });
            }
        }
    }
}