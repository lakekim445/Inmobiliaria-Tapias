namespace InmobiliariaAPI.DTOs
{
    public class CitaManualCreateDTO
    {
        /// <summary>Cliente registrado (0 si es un cliente nuevo sin cuenta).</summary>
        public int IdCliente { get; set; }
        public int IdPropiedad { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string? Observaciones { get; set; }

        // Datos del cliente sin cuenta (ej. contacto por WhatsApp)
        public string? NombreClienteNuevo { get; set; }
        public string? TelefonoClienteNuevo { get; set; }
        public string? EmailClienteNuevo { get; set; }
    }
}