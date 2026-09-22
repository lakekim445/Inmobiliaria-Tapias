namespace InmobiliariaAPI.Models
{
    public class EstadoCita
    {
        public int Id { get; set; }
        public string? NombreEstado { get; set; }
        public string? Descripcion { get; set; }
        public List<Cita>? Citas { get; set; }
    }
}