namespace tp5_Trani_Joaco_Alex.DTOs.Request
{
    public class ProductoCreateDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string? Imagen { get; set; }
        public int CategoriaId { get; set; }
    }
}