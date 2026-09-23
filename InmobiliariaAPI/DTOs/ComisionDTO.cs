namespace InmobiliariaAPI.DTOs
{
    public class ComisionDTO
    {
        public int IdPropiedad { get; set; }
        public string NombrePropiedad { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Zona { get; set; } = string.Empty;

        public string TipoOperacion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string Moneda { get; set; } = string.Empty;
        public DateTime? FechaCierre { get; set; }

        public int IdAgente { get; set; }
        public string NombreAgente { get; set; } = string.Empty;

        public decimal ComisionEmpresaPorcentaje { get; set; }
        public decimal ComisionEmpresaMonto { get; set; }

        public decimal ComisionAgentePorcentaje { get; set; }
        public decimal ComisionAgenteMonto { get; set; }

        public decimal GananciaNetaEmpresa { get; set; }
    }
}