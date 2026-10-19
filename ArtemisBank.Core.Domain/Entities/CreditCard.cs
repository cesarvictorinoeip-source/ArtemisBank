using ArtemisBank.Core.Domain.Common;

namespace ArtemisBank.Core.Domain.Entities
{
    public class CreditCard : BaseEntity
    {
        public required string CardNumber { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal AmountOwed { get; set; }
        public required string ExpirationDate { get; set; }
        public required string CVC { get; set; }
        public bool IsActive { get; set; }
        public required string UserId { get; set; }
        public required string AdminId { get; set; }

        public ICollection<CreditCardConsumption> Consumptions { get; set; } = new List<CreditCardConsumption>();
    }
}