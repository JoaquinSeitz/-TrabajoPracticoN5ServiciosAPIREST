namespace tp5_Trani_Joaco_Alex.DTOs.Response
{
    public class IngresoProductoResponseDTO
    {
        public int IngresoProductoId { get; set; }
        public DateTime Fecha { get; set; }
        public int Cantidad { get; set; }
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public int ProveedorId { get; set; }
        public string ProveedorNombre { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
    }
}