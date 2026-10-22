namespace tp5_Trani_Joaco_Alex.DTOs.Request
{
    public class RegistroUsuarioDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Rol { get; set; } = "Cliente"; // Por defecto si no envían rol
    }
}