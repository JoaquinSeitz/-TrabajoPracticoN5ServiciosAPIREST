using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
 using tp5_Trani_Joaco_Alex.Data;
 using tp5_Trani_Joaco_Alex.Models;

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

        // GET: api/Productos?pageNumber=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetProductos([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            int registrosASaltar = (pageNumber - 1) * pageSize;

            var productos = await _context.Productos
                .Skip(registrosASaltar)
                .Take(pageSize)
                .ToListAsync();

            return Ok(productos);
        }
    }
}