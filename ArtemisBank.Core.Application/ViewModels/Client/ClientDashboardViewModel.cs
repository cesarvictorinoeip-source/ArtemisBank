namespace ArtemisBank.Core.Application.ViewModels
{
    public class ClientDashboardViewModel
    {
        public List<SavingsAccountViewModel> SavingsAccounts { get; set; } = [];
        public List<LoanViewModel> Loans { get; set; } = [];
        public List<CreditCardViewModel> CreditCards { get; set; } = [];
    }
}