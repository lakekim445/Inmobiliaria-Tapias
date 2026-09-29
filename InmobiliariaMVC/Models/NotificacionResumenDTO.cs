namespace InmobiliariaMVC.Models
{
    public class NotificacionResumenDTO
    {
        public int Id { get; set; }
        public string? Tipo { get; set; }
        public string? Mensaje { get; set; }
        public bool Leida { get; set; }
        public DateTime FechaEnvio { get; set; }
        public int? IdCita { get; set; }
        public int? IdPropiedad { get; set; }
    }
}