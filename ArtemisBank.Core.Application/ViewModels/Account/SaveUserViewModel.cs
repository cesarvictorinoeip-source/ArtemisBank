using System.ComponentModel.DataAnnotations;

namespace ArtemisBank.Core.Application.ViewModels
{
    public class SaveUserViewModel
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "El nombre es requerido")]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "El apellido es requerido")]
        public string LastName { get; set; } = "";

        [Required(ErrorMessage = "La cédula es requerida")]
        public string IdCard { get; set; } = "";

        [Required(ErrorMessage = "El correo es requerido")]
        [EmailAddress(ErrorMessage = "El correo no es válido")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "El usuario es requerido")]
        public string UserName { get; set; } = "";

        [Required(ErrorMessage = "La contraseña es requerida")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Confirmar contraseña es requerido")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; } = "";

        [Required(ErrorMessage = "El tipo de usuario es requerido")]
        public string UserType { get; set; } = "";

        public decimal InitialAmount { get; set; }
        public decimal AdditionalAmount { get; set; }

        public bool HasError { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}