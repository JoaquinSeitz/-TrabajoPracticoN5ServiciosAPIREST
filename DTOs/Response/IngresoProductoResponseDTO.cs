using System;
using System.Collections.Generic;

namespace tp5_Trani_Joaco_Alex.DTOs.Response
{
    public class IngresoProductoResponseDTO
    {
        public int IngresoProductoId { get; set; }
        public DateTime Fecha { get; set; }
        public int ProveedorId { get; set; }
        public string ProveedorNombre { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
        public List<DetalleMovimientoResponseDTO> Detalles { get; set; } = new List<DetalleMovimientoResponseDTO>();
    }
}