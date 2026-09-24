using System.ComponentModel.DataAnnotations;

namespace InmobiliariaMVC.Models
{
    public class PropiedadCreateViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "El tipo es obligatorio")]
        public string Tipo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, 9999999)]
        public decimal Precio { get; set; }

        public string Moneda { get; set; } = "USD";

        [Required(ErrorMessage = "La zona es obligatoria")]
        public string Zona { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es obligatoria")]
        public string Direccion { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        [Range(0, 20)]
        public int Habitaciones { get; set; }

        [Range(0, 10)]
        public int Banos { get; set; }

        public decimal? SuperficieM2 { get; set; }

        public int IdAgente { get; set; }
    }
}