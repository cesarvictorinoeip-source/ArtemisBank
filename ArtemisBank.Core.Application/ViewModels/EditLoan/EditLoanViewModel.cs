namespace ArtemisBank.Core.Application.ViewModels
{
    public class EditLoanViewModel
    {
        public int Id { get; set; }
        public string LoanNumber { get; set; } = "";
        public decimal InterestRate { get; set; }
        public bool HasError { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}