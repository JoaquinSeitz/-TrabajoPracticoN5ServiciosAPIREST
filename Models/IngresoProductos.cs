using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace tp5_Trani_Joaco_Alex.Models
{
    public class IngresoProductos
    {
        [Key]
        public int IngresoProductoId { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        public int ProveedorId { get; set; }

        [ForeignKey("ProveedorId")]
        public Proveedores? Proveedor { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuarios? Usuario { get; set; }

        // Relación 1 a Muchos con los detalles
        public List<DetalleIngresoProductos> Detalles { get; set; } = new List<DetalleIngresoProductos>();
    }
}