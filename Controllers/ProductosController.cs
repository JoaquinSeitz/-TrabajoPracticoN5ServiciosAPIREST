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
    public class ProductosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProductos([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // Usamos Include para traer los datos de la Categoría y mapeamos al ResponseDTO
                var productos = await _context.Productos
                    .Include(p => p.Categoria)
                    .Where(p => p.Activo)
                    .Skip((pageNumber - 1) * pageSize)
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

        [HttpPost]
        public async Task<IActionResult> CrearProducto([FromBody] ProductoCreateDTO dto)
        {
            if (dto == null) return BadRequest(new { mensaje = "Los datos del producto son nulos." });

            try
            {
                // Construimos la entidad basándonos en los datos seguros del DTO
                var nuevoProducto = new Productos
                {
                    Nombre = dto.Nombre,
                    Precio = dto.Precio,
                    Stock = dto.Stock,
                    Imagen = dto.Imagen,
                    CategoriaId = dto.CategoriaId,
                    Activo = true // Nos aseguramos desde el backend que nazca activo
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
        public async Task<IActionResult> ActualizarProducto(int id, [FromBody] ProductoCreateDTO dto)
        {
            if (dto == null) return BadRequest(new { mensaje = "Los datos del producto son nulos." });

            try
            {
                var productoExistente = await _context.Productos.FindAsync(id);

                if (productoExistente == null || !productoExistente.Activo)
                    return NotFound(new { mensaje = "Producto no encontrado o inactivo." });

                // Pisamos únicamente los campos que permitimos actualizar
                productoExistente.Nombre = dto.Nombre;
                productoExistente.Precio = dto.Precio;
                productoExistente.Stock = dto.Stock;
                productoExistente.Imagen = dto.Imagen;
                productoExistente.CategoriaId = dto.CategoriaId;

                // No modificamos ni el ProductoId ni el campo Activo
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Producto actualizado exitosamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al actualizar el producto.", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            try
            {
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null) return NotFound(new { mensaje = "Producto no encontrado." });

                producto.Activo = false; // Soft Delete
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Producto eliminado lógicamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al eliminar el producto.", error = ex.Message });
            }
        }
    }
}