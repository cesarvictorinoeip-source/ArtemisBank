using System.ComponentModel.DataAnnotations;
namespace ArtemisBank.Core.Application.ViewModels
{
    public class LoanPaymentViewModel
    {
        [Required(ErrorMessage = "El número de cuenta es requerido")]
        public string AccountNumber { get; set; } = "";
        [Required(ErrorMessage = "El monto es requerido")]
        public decimal Amount { get; set; }
        [Required(ErrorMessage = "El número de préstamo es requerido")]
        public string LoanNumber { get; set; } = "";
        public string? OwnerFullName { get; set; }
        public bool HasError { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}