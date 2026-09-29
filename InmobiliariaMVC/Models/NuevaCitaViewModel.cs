namespace InmobiliariaMVC.Models
{
    public class NuevaCitaViewModel
    {
        // "registrado" = cliente con cuenta | "nuevo" = cliente sin cuenta (WhatsApp)
        public string Modo { get; set; } = "registrado";
        public int IdCliente { get; set; }
        public string? NombreClienteNuevo { get; set; }
        public string? TelefonoClienteNuevo { get; set; }
        public string? EmailClienteNuevo { get; set; }
        public int IdPropiedad { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Today.AddDays(1);
        public TimeSpan HoraInicio { get; set; } = new TimeSpan(9, 0, 0);
        public string? Observaciones { get; set; }

        // Datos para los selectores
        public List<ClienteResumenDTO> Clientes { get; set; } = new();
        public List<PropiedadResumenDTO> Propiedades { get; set; } = new();
    }
}