namespace InmobiliariaAPI.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string? NombreCompleto { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int? IdUsuario { get; set; }
        public int IdEstadoProspecto { get; set; }
        public Usuario? Usuario { get; set; }
        public EstadoProspecto? EstadoProspecto { get; set; }
        public List<Cita>? Citas { get; set; }
    }
}