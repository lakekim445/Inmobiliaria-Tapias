using System.ComponentModel.DataAnnotations;

namespace InmobiliariaMVC.Models
{
    public class AgenteCreateViewModel
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [StringLength(150, MinimumLength = 3)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [StringLength(20)]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mínimo 6 caracteres")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}