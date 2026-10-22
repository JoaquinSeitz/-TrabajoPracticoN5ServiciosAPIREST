using System;
using System.Collections.Generic;
using System.Linq;

namespace tp5_Trani_Joaco_Alex.DTOs.Response
{
    public class SalidaProductoResponseDTO
    {
        public int SalidaProductoId { get; set; }
        public DateTime Fecha { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
        public List<DetalleMovimientoResponseDTO> Detalles { get; set; } = new List<DetalleMovimientoResponseDTO>();

        // Suma automáticamente todos los subtotales para el ticket de venta
        public decimal Total => Detalles.Sum(d => d.Subtotal);
    }
}