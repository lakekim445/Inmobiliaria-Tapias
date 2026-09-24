namespace InmobiliariaAPI.DTOs
{
    public class PropiedadCreateDTO
    {
        public string Tipo { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string Moneda { get; set; } = "USD";
        public string Zona { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int Habitaciones { get; set; }
        public int Banos { get; set; }
        public decimal? SuperficieM2 { get; set; }
        public int IdAgente { get; set; }
        public List<ImagenPropiedadDTO> Imagenes { get; set; } = new();
    }
}