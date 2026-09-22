namespace InmobiliariaAPI.Models
{
    public class DisponibilidadAgente
    {
        public int Id { get; set; }
        public string? DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public bool Activo { get; set; }
        public int IdAgente { get; set; }
        public Usuario? Agente { get; set; }
        public List<Cita>? Citas { get; set; }
    }
}