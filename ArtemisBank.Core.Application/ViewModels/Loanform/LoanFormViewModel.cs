namespace ArtemisBank.Core.Application.ViewModels
{
    public class LoanFormViewModel
    {
        public string ClientId { get; set; } = "";
        public string ClientName { get; set; } = "";
        public decimal AverageDebt { get; set; }
        public int TermMonths { get; set; }
        public decimal Amount { get; set; }
        public decimal InterestRate { get; set; }
        public bool HasError { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}