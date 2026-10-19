namespace ArtemisBank.Core.Application.DTOs
{
    public class TransactionDto : BaseDto
    {
        public int SavingsAccountId { get; set; }
        public decimal Amount { get; set; }
        public required string Type { get; set; }
        public required string Beneficiary { get; set; }
        public required string Origin { get; set; }
        public required string Status { get; set; }
        public string? CashierId { get; set; }
    }
}