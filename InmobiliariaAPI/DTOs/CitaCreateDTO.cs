namespace InmobiliariaAPI.DTOs
{
    public class CitaCreateDTO
    {
        public int IdPropiedad { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string? Observaciones { get; set; }
    }
}