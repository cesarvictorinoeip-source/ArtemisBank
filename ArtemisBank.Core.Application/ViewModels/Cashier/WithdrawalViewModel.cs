using System.ComponentModel.DataAnnotations;
namespace ArtemisBank.Core.Application.ViewModels
{
    public class WithdrawalViewModel
    {
        [Required(ErrorMessage = "El número de cuenta es requerido")]
        public string AccountNumber { get; set; } = "";
        [Required(ErrorMessage = "El monto es requerido")]
        public decimal Amount { get; set; }
        public string? OwnerFullName { get; set; }
        public bool HasError { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}