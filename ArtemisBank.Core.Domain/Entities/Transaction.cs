using ArtemisBank.Core.Domain.Common;
namespace ArtemisBank.Core.Domain.Entities
{
    public class Transaction : BaseEntity
    {
        public int SavingsAccountId { get; set; }
        public decimal Amount { get; set; }
        public required string Type { get; set; }
        public required string Beneficiary { get; set; }
        public required string Origin { get; set; }
        public required string Status { get; set; }
        public string? CashierId { get; set; }
        public SavingsAccount? SavingsAccount { get; set; }
    }
}