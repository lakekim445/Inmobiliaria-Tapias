namespace InmobiliariaAPI.DTOs
{
    public class AgenteDetalleDTO
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string NombreRol { get; set; } = string.Empty;
    }
}