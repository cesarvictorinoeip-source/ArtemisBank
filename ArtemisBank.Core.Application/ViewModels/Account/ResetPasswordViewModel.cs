using System.ComponentModel.DataAnnotations;

namespace ArtemisBank.Core.Application.ViewModels
{
    public class ResetPasswordViewModel
    {
        public string Id { get; set; } = "";
        public string Token { get; set; } = "";

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Confirmar contraseña es requerido")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = "";

        public bool HasError { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}