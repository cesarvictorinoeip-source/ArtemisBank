namespace ArtemisBank.Core.Application.DTOs
{
    public class LoanInstallmentDto : BaseDto
    {
        public int LoanId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsPaid { get; set; }
        public bool IsLate { get; set; }
    }
}