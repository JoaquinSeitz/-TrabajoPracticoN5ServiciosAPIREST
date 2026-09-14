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

        // GET: api/Proveedores?pageNumber=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetProveedores([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var proveedores = await _context.Proveedores
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(proveedores);
        }
    }
}