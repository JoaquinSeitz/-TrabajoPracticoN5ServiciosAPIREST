using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using tp5_Trani_Joaco_Alex.Data;
using tp5_Trani_Joaco_Alex.Models;

namespace tp5_Trani_Joaco_Alex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public UsuariosController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO login)
        {
            // 1. Buscar el usuario en la base de datos
            var usuario = _context.Usuarios.FirstOrDefault(u =>
                u.Email == login.Email &&
                u.Password == login.Password &&
                u.Activo == true);

            if (usuario == null)
            {
                return Unauthorized(new { mensaje = "Email o contraseña incorrectos, o usuario inactivo." });
            }

            // 2. Crear los "Claims" (datos del usuario que viajan dentro del token)
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            // 3. Traer la clave secreta del appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 4. Configurar el Token
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            // 5. Generar el string final y devolverlo
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new { token = tokenString });
        }
    }
}