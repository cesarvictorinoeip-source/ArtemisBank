using System.ComponentModel.DataAnnotations;

namespace ArtemisBank.Core.Application.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "El usuario es requerido")]
        public string UserName { get; set; } = "";

        public bool HasError { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}