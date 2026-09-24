namespace InmobiliariaAPI.DTOs
{
    public class ImagenPropiedadDTO
    {
        public int Id { get; set; }
        public string UrlImagen { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool EsPrincipal { get; set; }
    }
}