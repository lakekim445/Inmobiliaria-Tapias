namespace InmobiliariaAPI.DTOs
{
    public class ClienteResumenDTO
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public string EstadoProspecto { get; set; } = string.Empty;
        public int OrdenEstado { get; set; }
    }
}