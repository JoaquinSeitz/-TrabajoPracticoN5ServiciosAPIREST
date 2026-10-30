using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tp5_Trani_Joaco_Alex.Data;
using tp5_Trani_Joaco_Alex.DTOs.Request;
using tp5_Trani_Joaco_Alex.DTOs.Response;
using tp5_Trani_Joaco_Alex.Models;

namespace tp5_Trani_Joaco_Alex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductosController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductos([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var productos = await _context.Productos
                     .Include(p => p.Categoria)
                     .Where(p => p.Activo)
                     .Skip((pageNumber - 1) * pageSize) // Corregido el error de sintaxis acá
                     .Take(pageSize)
                     .Select(p => new ProductoResponseDTO
                     {
                         ProductoId = p.ProductoId,
                         Nombre = p.Nombre,
                         Precio = p.Precio,
                         Stock = p.Stock,
                         Imagen = p.Imagen,
                         CategoriaId = p.CategoriaId,
                         CategoriaNombre = p.Categoria != null ? p.Categoria.Nombre : "Sin Categoría"
                     })
                     .ToListAsync();

                return Ok(productos);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error recuperando el listado de productos.", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProducto(int id)
        {
            try
            {
                var producto = await _context.Productos
                    .Include(p => p.Categoria)
                    .Where(p => p.ProductoId == id && p.Activo)
                    .Select(p => new ProductoResponseDTO
                    {
                        ProductoId = p.ProductoId,
                        Nombre = p.Nombre,
                        Precio = p.Precio,
                        Stock = p.Stock,
                        Imagen = p.Imagen,
                        CategoriaId = p.CategoriaId,
                        CategoriaNombre = p.Categoria != null ? p.Categoria.Nombre : "Sin Categoría"
                    })
                    .FirstOrDefaultAsync();

                if (producto == null)
                    return NotFound(new { mensaje = "Producto no encontrado o inactivo." });

                return Ok(producto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al obtener el producto.", error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> CrearProducto([FromBody] ProductoCreateDTO dto)
        {
            if (dto == null) return BadRequest(new { mensaje = "Los datos del producto son nulos." });

            // Iniciamos una transacción para garantizar que Producto e Ingreso se guarden juntos
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var nuevoProducto = new Productos
                {
                    Nombre = dto.Nombre,
                    Precio = dto.Precio,
                    Stock = dto.Stock,
                    Imagen = dto.Imagen,
                    CategoriaId = dto.CategoriaId,
                    Activo = true
                };

                _context.Productos.Add(nuevoProducto);
                await _context.SaveChangesAsync();

                // Si se crea con stock inicial > 0, registramos el movimiento en el historial real
                if (dto.Stock > 0)
                {
                    var ingresoInicial = new IngresoProductos
                    {
                        ProveedorId = dto.ProveedorId,
                        UsuarioId = 1,   // (Idealmente se saca del token JWT)
                        Fecha = DateTime.Now,
                        Detalles = new List<DetalleIngresoProductos>
                        {
                            new DetalleIngresoProductos
                            {
                                ProductoId = nuevoProducto.ProductoId,
                                Cantidad = dto.Stock
                            }
                        }
                    };

                    // ESTAS DOS LÍNEAS DEBEN ESTAR ADENTRO DEL IF
                    _context.IngresoProductos.Add(ingresoInicial);
                    await _context.SaveChangesAsync();
                }

                // Confirmamos la transacción
                await transaction.CommitAsync();

                return Ok(new { mensaje = "Producto creado exitosamente e historial actualizado.", ProductoId = nuevoProducto.ProductoId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al crear el producto.", error = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> ActualizarProducto(int id, [FromBody] ProductoCreateDTO dto)
        {
            if (dto == null) return BadRequest(new { mensaje = "Los datos del producto son nulos." });

            try
            {
                var productoExistente = await _context.Productos.FindAsync(id);

                if (productoExistente == null || !productoExistente.Activo)
                    return NotFound(new { mensaje = "Producto no encontrado o inactivo." });

                productoExistente.Nombre = dto.Nombre;
                productoExistente.Precio = dto.Precio;

                // Nota: El stock no debería modificarse desde un PUT, solo mediante ingresos/egresos, 
                // pero lo mantenemos si lo requieren las reglas de tu sistema.
                productoExistente.Stock = dto.Stock;
                productoExistente.Imagen = dto.Imagen;
                productoExistente.CategoriaId = dto.CategoriaId;

                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Producto actualizado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al actualizar el producto.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null) return NotFound(new { mensaje = "Producto no encontrado." });

                producto.Activo = false;
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Producto eliminado lógicamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al eliminar el producto.", error = ex.Message });
            }
        }

        // --- ENDPOINT PROFESIONAL PARA BLAZOR Y MAUI (MULTIPART/FORM-DATA) ---
        [HttpPost("{id}/imagen")]
        [Authorize(Roles = "Admin,Operador")]
        public async Task<IActionResult> SubirImagenReal(int id, IFormFile archivo)
        {
            try
            {
                if (archivo == null || archivo.Length == 0)
                    return BadRequest(new { mensaje = "No se envió ninguna imagen." });

                if (archivo.Length > 10 * 1024 * 1024)
                    return BadRequest(new { mensaje = "La imagen es muy pesada. Máximo 10MB." });

                var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(archivo.FileName).ToLower();

                if (!extensionesPermitidas.Contains(extension))
                    return BadRequest(new { mensaje = "Formato no permitido. Usá JPG, PNG o WEBP." });

                var producto = await _context.Productos.FindAsync(id);
                if (producto == null || !producto.Activo)
                    return NotFound(new { mensaje = "Producto no encontrado o inactivo." });

                string webRootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                string carpetaUploads = Path.Combine(webRootPath, "uploads");

                if (!Directory.Exists(carpetaUploads))
                    Directory.CreateDirectory(carpetaUploads);

                string nombreArchivo = $"prod_{id}_{Guid.NewGuid()}{extension}";
                string rutaFisica = Path.Combine(carpetaUploads, nombreArchivo);

                using (var stream = new FileStream(rutaFisica, FileMode.Create))
                {
                    await archivo.CopyToAsync(stream);
                }

                producto.Imagen = $"/uploads/{nombreArchivo}";
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    mensaje = "Imagen subida exitosamente.",
                    ruta = producto.Imagen
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    mensaje = "Error fatal al guardar la imagen.",
                    error = ex.Message
                });
            }
        }

        // ==============================================================
        // NUEVOS ENDPOINTS PARA MANTENIMIENTO WEB (SOLO ADMIN)
        // ==============================================================

        [HttpGet("inactivos")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetProductosInactivos()
        {
            try
            {
                var productos = await _context.Productos
                     .Include(p => p.Categoria)
                     .Where(p => !p.Activo)
                     .OrderBy(p => p.Nombre)
                     .Select(p => new ProductoResponseDTO
                     {
                         ProductoId = p.ProductoId,
                         Nombre = p.Nombre,
                         Precio = p.Precio,
                         Stock = p.Stock,
                         Imagen = p.Imagen,
                         CategoriaId = p.CategoriaId,
                         CategoriaNombre = p.Categoria != null ? p.Categoria.Nombre : "Sin Categoría"
                     })
                     .ToListAsync();

                return Ok(productos);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error recuperando inactivos.", error = ex.Message });
            }
        }

        [HttpPut("reactivar/{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> ReactivarProducto(int id)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null) return NotFound(new { mensaje = "Producto no encontrado." });

                producto.Activo = true;
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Producto dado de alta nuevamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al reactivar.", error = ex.Message });
            }
        }

        [HttpPut("actualizar-stock/{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> ActualizarStockManual(int id, [FromBody] NuevoStockDTO dto)
        {
            if (dto == null || dto.NuevoStock < 0)
                return BadRequest(new { mensaje = "Stock inválido." });

            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null || !producto.Activo)
                    return NotFound(new { mensaje = "Producto no encontrado o inactivo." });

                producto.Stock = dto.NuevoStock;
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Stock actualizado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al modificar stock.", error = ex.Message });
            }
        }
        [HttpPut("actualizar-precio/{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> ActualizarPrecioManual(int id, [FromBody] NuevoPrecioDTO dto)
        {
            if (dto == null || dto.NuevoPrecio <= 0)
                return BadRequest(new { mensaje = "Precio inválido. Debe ser mayor a 0." });

            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null || !producto.Activo)
                    return NotFound(new { mensaje = "Producto no encontrado o inactivo." });

                producto.Precio = dto.NuevoPrecio;
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Precio actualizado correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al modificar precio.", error = ex.Message });
            }
        }
    

    // DTO simple auxiliar para recibir el nuevo stock en el JSON (Ya lo tienes)
    public class NuevoStockDTO
    {
        public int NuevoStock { get; set; }
    }

    // NUEVO DTO auxiliar para recibir el nuevo precio
    public class NuevoPrecioDTO
    {
        public decimal NuevoPrecio { get; set; }
    }

}

    // DTO simple auxiliar para recibir el nuevo stock en el JSON
    public class NuevoStockDTO
    {
        public int NuevoStock { get; set; }
    }
}