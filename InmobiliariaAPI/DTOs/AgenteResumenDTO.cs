namespace InmobiliariaAPI.DTOs
{
    public class AgenteResumenDTO
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }

        public int TotalPropiedades { get; set; }
        public int PropiedadesDisponibles { get; set; }
        public int PropiedadesVendidas { get; set; }
        public int PropiedadesAlquiladas { get; set; }
        public int PropiedadesAnticretico { get; set; }

        public decimal ComisionTotal { get; set; }
    }
}