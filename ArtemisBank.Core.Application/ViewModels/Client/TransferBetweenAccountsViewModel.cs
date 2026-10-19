using System.ComponentModel.DataAnnotations;
namespace ArtemisBank.Core.Application.ViewModels
{
    public class TransferBetweenAccountsViewModel
    {
        [Required(ErrorMessage = "La cuenta de origen es requerida")]
        public int OriginAccountId { get; set; }
        [Required(ErrorMessage = "La cuenta de destino es requerida")]
        public int DestinationAccountId { get; set; }
        [Required(ErrorMessage = "El monto es requerido")]
        public decimal Amount { get; set; }
        public List<SavingsAccountViewModel> Accounts { get; set; } = [];
        public bool HasError { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}