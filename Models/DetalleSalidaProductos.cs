using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace tp5_Trani_Joaco_Alex.Models
{
    public class DetalleSalidaProductos
    {
        [Key]
        public int DetalleSalidaId { get; set; }

        [Required]
        public int SalidaProductoId { get; set; }

        [ForeignKey("SalidaProductoId")]
        [JsonIgnore] // Evita ciclos infinitos al convertir a JSON
        public SalidaProductos? SalidaProducto { get; set; }

        [Required]
        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public Productos? Producto { get; set; }

        [Required]
        public int Cantidad { get; set; }
       
        [Required]
        public decimal PrecioUnitario { get; set; }
    }
}