namespace tp5_Trani_Joaco_Alex.DTOs.Response
{
    public class DetalleMovimientoResponseDTO
    {
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal => Cantidad * PrecioUnitario; // Se calcula solo
    }
}