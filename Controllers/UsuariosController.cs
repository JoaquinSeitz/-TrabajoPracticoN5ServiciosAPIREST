using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using tp5_Trani_Joaco_Alex.Data;
using tp5_Trani_Joaco_Alex.DTOs.Request;
using tp5_Trani_Joaco_Alex.DTOs.Response;
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

        [HttpGet]
        [Authorize(Roles = Roles.Admin)] // Solo el Admin puede listar los usuarios
        public IActionResult ObtenerUsuarios()
        {
            try
            {
                var usuarios = _context.Usuarios
                    .Where(u => u.Activo == true)
                    .Select(u => new UsuarioResponseDTO
                    {
                        UsuarioId = u.UsuarioId,
                        Nombre = u.Nombre,
                        Email = u.Email,
                        Rol = u.Rol
                    })
                    .ToList();

                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error recuperando los usuarios.", error = ex.Message });
            }
        }

        [HttpPost("registro")]
        [AllowAnonymous]
        public IActionResult Registrar([FromBody] RegistroUsuarioDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { mensaje = "Datos de registro incompletos." });

            try
            {
                if (_context.Usuarios.Any(u => u.Email == dto.Email))
                    return BadRequest(new { mensaje = "El email ya está registrado." });

                // Validamos que el rol sea uno de los permitidos (Admin, Operador o Cliente)
                var rolAsignado = (dto.Rol == Roles.Admin || dto.Rol == Roles.Operador)
                    ? dto.Rol
                    : "Cliente";

                var nuevoUsuario = new Usuarios
                {
                    Nombre = dto.Nombre,
                    Email = dto.Email,
                    Password = dto.Password,
                    Rol = rolAsignado,
                    Activo = true
                };

                _context.Usuarios.Add(nuevoUsuario);
                _context.SaveChanges();

                // Si se registra como Cliente (App Móvil), también lo damos de alta en la tabla Clientes
                if (rolAsignado == "Cliente" && !_context.Clientes.Any(c => c.Email == dto.Email))
                {
                    _context.Clientes.Add(new Clientes
                    {
                        Nombre = dto.Nombre,
                        Email = dto.Email,
                        Telefono = "Sin especificar",
                        Activo = true
                    });
                    _context.SaveChanges();
                }

                return Ok(new { mensaje = "Usuario registrado exitosamente.", UsuarioId = nuevoUsuario.UsuarioId, Rol = nuevoUsuario.Rol });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error al registrar el usuario.", error = ex.Message });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginDTO login)
        {
            if (login == null) return BadRequest(new { mensaje = "Datos de inicio de sesión nulos." });

            try
            {
                var usuario = _context.Usuarios.FirstOrDefault(u =>
                    u.Email == login.Email &&
                    u.Password == login.Password &&
                    u.Activo == true);

                if (usuario == null)
                {
                    return Unauthorized(new { mensaje = "Email o contraseña incorrectos, o usuario inactivo." });
                }

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Nombre),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.Role, usuario.Rol)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(2),
                    signingCredentials: creds
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                // Devolvemos el token junto con los datos básicos para usar directo en Blazor
                return Ok(new
                {
                    token = tokenString,
                    usuarioId = usuario.UsuarioId,
                    nombre = usuario.Nombre,
                    email = usuario.Email,
                    rol = usuario.Rol
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "Error interno durante el inicio de sesión.", error = ex.Message });
            }
        }
    }
}