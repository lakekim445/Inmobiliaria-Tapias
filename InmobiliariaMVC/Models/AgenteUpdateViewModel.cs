using System.ComponentModel.DataAnnotations;

namespace InmobiliariaMVC.Models
{
    public class AgenteUpdateViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [StringLength(150, MinimumLength = 3)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [StringLength(20)]
        public string Telefono { get; set; } = string.Empty;

        public bool Activo { get; set; }
    }
}