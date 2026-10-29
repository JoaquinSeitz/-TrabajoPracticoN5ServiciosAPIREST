using System.ComponentModel.DataAnnotations;

namespace tp5_Trani_Joaco_Alex.DTOs.Request
{
    public class ProductoCreateDTO
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public decimal Precio { get; set; }

        public int Stock { get; set; }

        public string? Imagen { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una categoría.")]
        public int CategoriaId { get; set; }

        // NUEVO CAMPO REQUERIDO
        [Required(ErrorMessage = "Debe seleccionar un proveedor.")]
        [Range(1, int.MaxValue, ErrorMessage = "Proveedor inválido.")]
        public int ProveedorId { get; set; }
    }
}