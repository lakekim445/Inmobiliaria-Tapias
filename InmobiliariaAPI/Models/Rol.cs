namespace InmobiliariaAPI.Models
{
    public class Rol
    {
        public int Id { get; set; }
        public string? NombreRol { get; set; }
        public string? Descripcion { get; set; }
        public List<Usuario>? Usuarios { get; set; }
    }
}