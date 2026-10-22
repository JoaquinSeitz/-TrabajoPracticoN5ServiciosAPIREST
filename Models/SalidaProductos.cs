using System;
using System.Collections.Generic;
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
        public int ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public Clientes? Cliente { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuarios? Usuario { get; set; }

        // Relación 1 a Muchos con los detalles
        public List<DetalleSalidaProductos> Detalles { get; set; } = new List<DetalleSalidaProductos>();
    }
}