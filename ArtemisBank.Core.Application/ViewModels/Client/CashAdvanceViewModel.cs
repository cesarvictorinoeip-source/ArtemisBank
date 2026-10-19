using System.ComponentModel.DataAnnotations;
namespace ArtemisBank.Core.Application.ViewModels
{
    public class CashAdvanceViewModel
    {
        [Required(ErrorMessage = "La tarjeta es requerida")]
        public int CardId { get; set; }
        [Required(ErrorMessage = "La cuenta destino es requerida")]
        public int DestinationAccountId { get; set; }
        [Required(ErrorMessage = "El monto es requerido")]
        public decimal Amount { get; set; }
        public List<CreditCardViewModel> Cards { get; set; } = [];
        public List<SavingsAccountViewModel> DestinationAccounts { get; set; } = [];
        public bool HasError { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}