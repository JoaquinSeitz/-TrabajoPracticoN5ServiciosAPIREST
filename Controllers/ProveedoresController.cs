using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tp5_Trani_Joaco_Alex.Data;
using tp5_Trani_Joaco_Alex.Models;

namespace tp5_Trani_Joaco_Alex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            var proveedores = await _context.Proveedores
                .Where(p => p.Activo)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return Ok(proveedores);
        }

        [HttpPost]
        public async Task<IActionResult> CrearProveedor([FromBody] Proveedores proveedor)
        {
            _context.Proveedores.Add(proveedor);
            await _context.SaveChangesAsync();
            return Ok(new { Mensaje = "Proveedor creado", Proveedor = proveedor });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarProveedor(int id, [FromBody] Proveedores proveedor)
        {
            if (id != proveedor.ProveedorId) return BadRequest("ID incorrecto.");
            _context.Entry(proveedor).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok(new { Mensaje = "Proveedor actualizado" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarProveedor(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null) return NotFound();

            proveedor.Activo = false;
            await _context.SaveChangesAsync();
            return Ok(new { Mensaje = "Proveedor eliminado" });
        }
    }
}