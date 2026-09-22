namespace InmobiliariaAPI.Models
{
    public class HistorialEstadoCita
    {
        public int Id { get; set; }
        public int? EstadoAnterior { get; set; }
        public int EstadoNuevo { get; set; }
        public string? Motivo { get; set; }
        public DateTime FechaCambio { get; set; }
        public int IdCita { get; set; }
        public int IdUsuario { get; set; }
        public Cita? Cita { get; set; }
        public Usuario? Usuario { get; set; }
    }
}