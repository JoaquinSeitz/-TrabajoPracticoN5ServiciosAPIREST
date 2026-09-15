using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tp5_Trani_Joaco_Alex.Models
{
    public class Productos
    {
        [Key]
        public int ProductoId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Precio { get; set; }

        public int Stock { get; set; } = 0;

        [MaxLength(500)]
        public string? Imagen { get; set; }

        [Required]
        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public Categoria? Categoria { get; set; }

        public bool Activo { get; set; } = true;
    }
}