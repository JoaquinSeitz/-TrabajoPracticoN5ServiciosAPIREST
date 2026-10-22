using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace tp5_Trani_Joaco_Alex.Models
{
    public class DetalleIngresoProductos
    {
        [Key]
        public int DetalleIngresoId { get; set; }

        [Required]
        public int IngresoProductoId { get; set; }

        [ForeignKey("IngresoProductoId")]
        [JsonIgnore]
        public IngresoProductos? IngresoProducto { get; set; }

        [Required]
        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public Productos? Producto { get; set; }

        [Required]
        public int Cantidad { get; set; }

        // Nueva propiedad obligatoria para 3FN (congela el precio al momento del ingreso)
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }
    }
}