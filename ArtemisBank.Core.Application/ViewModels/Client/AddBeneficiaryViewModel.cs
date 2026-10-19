using System.ComponentModel.DataAnnotations;
namespace ArtemisBank.Core.Application.ViewModels
{
    public class AddBeneficiaryViewModel
    {
        [Required(ErrorMessage = "El número de cuenta es requerido")]
        public string AccountNumber { get; set; } = "";
        public bool HasError { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}