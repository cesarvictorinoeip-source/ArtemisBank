using System.ComponentModel.DataAnnotations;
namespace ArtemisBank.Core.Application.ViewModels
{
    public class BeneficiaryTransactionViewModel
    {
        [Required(ErrorMessage = "El beneficiario es requerido")]
        public int BeneficiaryId { get; set; }
        [Required(ErrorMessage = "El monto es requerido")]
        public decimal Amount { get; set; }
        [Required(ErrorMessage = "La cuenta de origen es requerida")]
        public int OriginAccountId { get; set; }
        public string? DestinationOwnerFullName { get; set; }
        public string? DestinationAccountNumber { get; set; }
        public List<BeneficiaryViewModel> Beneficiaries { get; set; } = [];
        public List<SavingsAccountViewModel> OriginAccounts { get; set; } = [];
        public bool HasError { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}