namespace InmobiliariaMVC.Models
{
    public class ReservaViewModel
    {
        public int IdPropiedad { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Today.AddDays(1);
        public TimeSpan HoraInicio { get; set; } = new TimeSpan(9, 0, 0);
        public string? Observaciones { get; set; }

        // Datos de la propiedad (solo lectura, para mostrar)
        public string Tipo { get; set; } = string.Empty;
        public string Zona { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string Moneda { get; set; } = "USD";
        public string? UrlImagen { get; set; }
    }
}