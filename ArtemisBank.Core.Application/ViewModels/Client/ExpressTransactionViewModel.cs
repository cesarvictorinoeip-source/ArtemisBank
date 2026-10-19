using System.ComponentModel.DataAnnotations;
namespace ArtemisBank.Core.Application.ViewModels
{
    public class ExpressTransactionViewModel
    {
        [Required(ErrorMessage = "El número de cuenta destino es requerido")]
        public string DestinationAccountNumber { get; set; } = "";
        [Required(ErrorMessage = "El monto es requerido")]
        public decimal Amount { get; set; }
        [Required(ErrorMessage = "La cuenta de origen es requerida")]
        public int OriginAccountId { get; set; }
        public string? DestinationOwnerFullName { get; set; }
        public List<SavingsAccountViewModel> OriginAccounts { get; set; } = [];
        public bool HasError { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}