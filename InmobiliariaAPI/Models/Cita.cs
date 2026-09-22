namespace InmobiliariaAPI.Models
{
    public class Cita
    {
        public int Id { get; set; }
        public DateTime FechaCita { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public string? Observaciones { get; set; }
        public int IdCliente { get; set; }
        public int IdPropiedad { get; set; }
        public int IdAgente { get; set; }
        public int IdDisponibilidad { get; set; }
        public int IdEstadoCita { get; set; }
        public Cliente? Cliente { get; set; }
        public Propiedad? Propiedad { get; set; }
        public Usuario? Agente { get; set; }
        public DisponibilidadAgente? Disponibilidad { get; set; }
        public EstadoCita? EstadoCita { get; set; }
    }
}