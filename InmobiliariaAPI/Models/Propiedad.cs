namespace InmobiliariaAPI.Models
{
    public class Propiedad
    {
        public int Id { get; set; }
        public string? Tipo { get; set; }
        public decimal Precio { get; set; }
        public string? Moneda { get; set; }
        public string? Zona { get; set; }
        public string? Direccion { get; set; }
        public string? Descripcion { get; set; }
        public int Habitaciones { get; set; }
        public int Banos { get; set; }
        public decimal? SuperficieM2 { get; set; }
        public string? Estado { get; set; }
        public DateTime FechaPublicacion { get; set; }
        public int IdAgente { get; set; }
        public Usuario? Agente { get; set; }
        public List<ImagenPropiedad>? Imagenes { get; set; }
        public List<Cita>? Citas { get; set; }
    }
}