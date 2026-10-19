using System.ComponentModel.DataAnnotations;
namespace ArtemisBank.Core.Application.ViewModels
{
    public class LoanPaymentClientViewModel
    {
        [Required(ErrorMessage = "El préstamo es requerido")]
        public int LoanId { get; set; }
        [Required(ErrorMessage = "La cuenta de origen es requerida")]
        public int OriginAccountId { get; set; }
        [Required(ErrorMessage = "El monto es requerido")]
        public decimal Amount { get; set; }
        public List<LoanViewModel> Loans { get; set; } = [];
        public List<SavingsAccountViewModel> OriginAccounts { get; set; } = [];
        public bool HasError { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}