namespace InmobiliariaMVC.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string? NombreCompleto { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }   // ← guarda texto plano ahora
        public string? Telefono { get; set; }
        public DateTime FechaRegistro { get; set; }
        public bool Activo { get; set; }
        public int IdRol { get; set; }
        public Rol? Rol { get; set; }
    }
}