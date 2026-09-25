using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tp5_Trani_Joaco_Alex.Data;
using tp5_Trani_Joaco_Alex.Models;

namespace tp5_Trani_Joaco_Alex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Esto bloquea todo el controlador.
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Clientes?pagina=1&cantidad=10 (Listado con Paginado)
        [HttpGet]
        public IActionResult GetClientes([FromQuery] int pagina = 1, [FromQuery] int cantidad = 10)
        {
            try
            {
                // Solo traemos los clientes activos
                var query = _context.Clientes.Where(c => c.Activo == true);

                var totalRegistros = query.Count();

                // Lógica matemática del paginado
                var clientes = query
                    .Skip((pagina - 1) * cantidad)
                    .Take(cantidad)
                    .ToList();

                return Ok(new
                {
                    totalRegistros,
                    paginaActual = pagina,
                    registrosPorPagina = cantidad,
                    datos = clientes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error recuperando el listado de clientes.", error = ex.Message });
            }
        }

        // POST: api/Clientes (Alta)
        [HttpPost]
        public IActionResult CrearCliente([FromBody] Clientes cliente)
        {
            if (cliente == null) return BadRequest(new { mensaje = "El cliente proporcionado es nulo." });

            try
            {
                cliente.Activo = true; // Forzamos que nazca activo
                _context.Clientes.Add(cliente);
                _context.SaveChanges();

                return Ok(new { mensaje = "Cliente creado con éxito.", cliente });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al crear el cliente.", error = ex.Message });
            }
        }

        // PUT: api/Clientes/5 (Modificación)
        [HttpPut("{id}")]
        public IActionResult ModificarCliente(int id, [FromBody] Clientes clienteActualizado)
        {
            if (clienteActualizado == null) return BadRequest(new { mensaje = "Datos inválidos." });

            try
            {
                var cliente = _context.Clientes.FirstOrDefault(c => c.ClienteId == id && c.Activo == true);

                if (cliente == null)
                {
                    return NotFound(new { mensaje = "Cliente no encontrado o inactivo." });
                }

                // Actualizamos los datos
                cliente.Nombre = clienteActualizado.Nombre;
                cliente.Telefono = clienteActualizado.Telefono;
                cliente.Email = clienteActualizado.Email;

                _context.SaveChanges();

                return Ok(new { mensaje = "Cliente actualizado correctamente.", cliente });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al modificar el cliente.", error = ex.Message });
            }
        }

        // DELETE: api/Clientes/5 (Baja Lógica / Soft Delete)
        [HttpDelete("{id}")]
        public IActionResult EliminarCliente(int id)
        {
            try
            {
                var cliente = _context.Clientes.FirstOrDefault(c => c.ClienteId == id);

                if (cliente == null)
                {
                    return NotFound(new { mensaje = "Cliente no encontrado." });
                }

                // Soft Delete: En vez de usar _context.Clientes.Remove(cliente), le cambiamos el estado
                cliente.Activo = false;
                _context.SaveChanges();

                return Ok(new { mensaje = "Cliente eliminado lógicamente (Soft Delete)." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al eliminar el cliente.", error = ex.Message });
            }
        }
    }
}