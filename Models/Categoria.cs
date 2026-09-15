using System.ComponentModel.DataAnnotations;

namespace tp5_Trani_Joaco_Alex.Models
{
    public class Categoria
    {
        [Key]
        public int CategoriaId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Descripcion { get; set; }

        public bool Activo { get; set; } = true;
    }
}