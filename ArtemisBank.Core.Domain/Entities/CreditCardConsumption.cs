using ArtemisBank.Core.Domain.Common;

namespace ArtemisBank.Core.Domain.Entities
{
    public class CreditCardConsumption : BaseEntity
    {
        public int CreditCardId { get; set; }
        public decimal Amount { get; set; }
        public required string Commerce { get; set; }
        public required string Status { get; set; }

        public required CreditCard CreditCard { get; set; }
    }
}