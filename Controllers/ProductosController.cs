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
        private readonly IWebHostEnvironment _env; // Inyectamos el entorno para leer wwwroot

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
                    .Skip((pageNumber - 1) * pageSize) // Error de tipeo corregido
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

                return Ok(new { mensaje = "Producto creado exitosamente.", ProductoId = nuevoProducto.ProductoId });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al crear el producto.", error = ex.Message });
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

                // Validamos tamaño máximo manual por seguridad (ej: 10 MB)
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

                // Copiamos el archivo real de forma eficiente (Streaming)
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
    }
}