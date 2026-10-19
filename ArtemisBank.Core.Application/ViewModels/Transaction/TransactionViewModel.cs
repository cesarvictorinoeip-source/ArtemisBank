namespace ArtemisBank.Core.Application.ViewModels
{
    public class TransactionViewModel
    {
        public int Id { get; set; }
        public int SavingsAccountId { get; set; }
        public decimal Amount { get; set; }
        public required string Type { get; set; }
        public required string Beneficiary { get; set; }
        public required string Origin { get; set; }
        public required string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}