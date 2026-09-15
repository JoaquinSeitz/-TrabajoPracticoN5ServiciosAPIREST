using System.ComponentModel.DataAnnotations;

namespace tp5_Trani_Joaco_Alex.Models
{
    public class Proveedores
    {
        [Key]
        public int ProveedorId { get; set; }

        [Required]
        [MaxLength(150)]
        public string RazonSocial { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Telefono { get; set; }

        [MaxLength(150)]
        public string? Email { get; set; }

        public bool Activo { get; set; } = true;
    }
}