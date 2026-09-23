namespace InmobiliariaAPI.DTOs
{
    public class CerrarOperacionDTO
    {
        public string TipoOperacion { get; set; } = string.Empty;
        public decimal PrecioFinal { get; set; }
        public string? Observaciones { get; set; }
    }
}