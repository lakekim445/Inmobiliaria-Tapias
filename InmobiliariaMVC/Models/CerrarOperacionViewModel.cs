using System.ComponentModel.DataAnnotations;

namespace InmobiliariaMVC.Models
{
    public class CerrarOperacionViewModel
    {
        public int Id { get; set; }
        public string NombrePropiedad { get; set; } = string.Empty;
        public decimal PrecioPublicado { get; set; }
        public string Moneda { get; set; } = "USD";

        [Required(ErrorMessage = "El tipo de operación es obligatorio")]
        public string TipoOperacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio final es obligatorio")]
        [Range(0.01, 9999999)]
        public decimal PrecioFinal { get; set; }

        public string? Observaciones { get; set; }
    }
}