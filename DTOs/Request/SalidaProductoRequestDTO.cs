using System.Collections.Generic;

namespace tp5_Trani_Joaco_Alex.DTOs.Request
{
    public class SalidaProductoRequestDTO
    {
        public int ClienteId { get; set; }
        public int UsuarioId { get; set; }
        public List<DetalleMovimientoRequestDTO> Detalles { get; set; } = new List<DetalleMovimientoRequestDTO>();
    }
}