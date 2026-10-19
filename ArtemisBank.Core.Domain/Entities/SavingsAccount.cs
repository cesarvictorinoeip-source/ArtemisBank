using ArtemisBank.Core.Domain.Common;

namespace ArtemisBank.Core.Domain.Entities
{
    public class SavingsAccount : BaseEntity
    {
        public required string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
        public required string UserId { get; set; }

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}