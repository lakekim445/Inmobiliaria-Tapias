namespace InmobiliariaMVC.Models
{
    public class CitaResumenDTO
    {
        public int Id { get; set; }
        public DateTime FechaCita { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public string? Observaciones { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteTelefono { get; set; } = string.Empty;
        public string ClienteEmail { get; set; } = string.Empty;
        public string PropiedadTipo { get; set; } = string.Empty;
        public string PropiedadZona { get; set; } = string.Empty;
        public string PropiedadDireccion { get; set; } = string.Empty;
        public string AgenteNombre { get; set; } = string.Empty;
        public string EstadoNombre { get; set; } = string.Empty;
    }
}