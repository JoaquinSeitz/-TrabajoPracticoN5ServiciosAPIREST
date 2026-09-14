using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tp5_Trani_Joaco_Alex.Models
{
    public class SalidaProductos
    {
        [Key]
        public int SalidaProductoId { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        public int Cantidad { get; set; }

        [Required]
        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public Productos? Producto { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public Clientes? Cliente { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuarios? Usuario { get; set; }
    }
}