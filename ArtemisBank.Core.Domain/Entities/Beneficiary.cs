using ArtemisBank.Core.Domain.Common;

namespace ArtemisBank.Core.Domain.Entities
{
    public class Beneficiary : BaseEntity
    {
        public required string UserId { get; set; }
        public required string AccountNumber { get; set; }
    }
}