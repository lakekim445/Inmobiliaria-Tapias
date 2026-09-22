namespace InmobiliariaAPI.Models
{
    public class SeguimientoCliente
    {
        public int Id { get; set; }
        public DateTime FechaInteraccion { get; set; }
        public string? TipoContacto { get; set; }
        public string? Descripcion { get; set; }
        public string? ProximaAccion { get; set; }
        public DateTime? FechaProxima { get; set; }
        public int IdCliente { get; set; }
        public int IdAgente { get; set; }
        public int? IdPropiedad { get; set; }
        public Cliente? Cliente { get; set; }
        public Usuario? Agente { get; set; }
        public Propiedad? Propiedad { get; set; }
    }
}