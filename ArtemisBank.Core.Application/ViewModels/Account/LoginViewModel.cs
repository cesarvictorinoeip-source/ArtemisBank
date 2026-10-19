using System.ComponentModel.DataAnnotations;

namespace ArtemisBank.Core.Application.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El usuario es requerido")]
        public string UserName { get; set; } = "";

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        public bool HasError { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}