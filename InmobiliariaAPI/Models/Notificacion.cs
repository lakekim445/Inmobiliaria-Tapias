namespace InmobiliariaAPI.Models
{
    public class Notificacion
    {
        public int Id { get; set; }
        public string? Tipo { get; set; }
        public string? Mensaje { get; set; }
        public bool Leida { get; set; }
        public DateTime FechaEnvio { get; set; }
        public DateTime? FechaLectura { get; set; }
        public int IdUsuario { get; set; }
        public int? IdCita { get; set; }
        public int? IdPropiedad { get; set; }
        public Usuario? Usuario { get; set; }
        public Cita? Cita { get; set; }
        public Propiedad? Propiedad { get; set; }
    }
}