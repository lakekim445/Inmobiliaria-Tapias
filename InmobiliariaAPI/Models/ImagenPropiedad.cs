namespace InmobiliariaAPI.Models
{
    public class ImagenPropiedad
    {
        public int Id { get; set; }
        public string? UrlImagen { get; set; }
        public string? Descripcion { get; set; }
        public bool EsPrincipal { get; set; }
        public int IdPropiedad { get; set; }
        public Propiedad? Propiedad { get; set; }
    }
}