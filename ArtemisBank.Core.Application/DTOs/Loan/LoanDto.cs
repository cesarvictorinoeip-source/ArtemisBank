namespace ArtemisBank.Core.Application.DTOs
{
    public class LoanDto : BaseDto
    {
        public required string LoanNumber { get; set; }
        public decimal Amount { get; set; }
        public decimal InterestRate { get; set; }
        public int TermMonths { get; set; }
        public int TotalInstallments { get; set; }
        public int PaidInstallments { get; set; }
        public decimal PendingAmount { get; set; }
        public bool IsActive { get; set; }
        public bool IsLate { get; set; }
        public required string UserId { get; set; }
        public string? OwnerFullName { get; set; }
        public string? AdminId { get; set; }
        public List<LoanInstallmentDto> Installments { get; set; } = [];
    }
}