using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using tp5_Trani_Joaco_Alex.Data;
 using tp5_Trani_Joaco_Alex.Models;

namespace tp5_Trani_Joaco_Alex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Categorias?pageNumber=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetCategorias([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var categorias = await _context.Categorias
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(categorias);
        }
    }
}