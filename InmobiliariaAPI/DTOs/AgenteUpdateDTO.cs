namespace InmobiliariaAPI.DTOs
{
    public class AgenteUpdateDTO
    {
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}