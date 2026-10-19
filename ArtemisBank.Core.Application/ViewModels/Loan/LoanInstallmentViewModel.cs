namespace ArtemisBank.Core.Application.ViewModels
{
    public class LoanInstallmentViewModel
    {
        public int Id { get; set; }
        public int LoanId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsPaid { get; set; }
        public bool IsLate { get; set; }
    }
}