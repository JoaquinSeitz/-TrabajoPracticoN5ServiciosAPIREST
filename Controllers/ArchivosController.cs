using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace tp5_Trani_Joaco_Alex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Protegemos también la subida de archivos
    public class ArchivosController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        // Inyectamos esto para que la API sepa dónde está físicamente la carpeta del proyecto
        public ArchivosController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost("subir-imagen")]
        public async Task<IActionResult> SubirImagen(IFormFile imagen)
        {
            // 1. Validar que venga un archivo (Esto queda fuera del try porque es una regla de negocio básica)
            if (imagen == null || imagen.Length == 0)
            {
                return BadRequest(new { mensaje = "No se envió ninguna imagen." });
            }

            try
            {
                // 2. Definir la ruta física: wwwroot/uploads
                // Si _env.WebRootPath es null, construimos la ruta manualmente
                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var uploadsFolder = Path.Combine(webRoot, "uploads");

                // 3. Crear la carpeta si no existe
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // 4. Generar un nombre único para evitar que dos imágenes se llamen igual y se pisen
                var nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
                var rutaCompleta = Path.Combine(uploadsFolder, nombreArchivo);

                // 5. Guardar el archivo físicamente en el disco
                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await imagen.CopyToAsync(stream);
                }

                // 6. Devolver la ruta que se va a guardar en la tabla Productos de SQL Server
                var rutaRelativa = $"/uploads/{nombreArchivo}";

                return Ok(new
                {
                    mensaje = "Imagen subida con éxito",
                    ruta = rutaRelativa
                });
            }
            catch (Exception ex)
            {
                // Devolvemos el Error 500 si falla la escritura en disco
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error interno al guardar la imagen en el servidor.", error = ex.Message });
            }
        }
    }
}