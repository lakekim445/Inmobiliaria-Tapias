namespace InmobiliariaMVC.Models
{
    public class PropiedadResumenDTO
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string Moneda { get; set; } = string.Empty;
        public string Zona { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int Habitaciones { get; set; }
        public int Banos { get; set; }
        public decimal? SuperficieM2 { get; set; }
        public DateTime FechaPublicacion { get; set; }

        public int IdAgente { get; set; }
        public string NombreAgente { get; set; } = string.Empty;

        public string? TipoOperacion { get; set; }
        public DateTime? FechaCierre { get; set; }
        public decimal? ComisionEmpresaPorcentaje { get; set; }
        public decimal? ComisionAgentePorcentaje { get; set; }

        public string? UrlImagenPrincipal { get; set; }
    }
}