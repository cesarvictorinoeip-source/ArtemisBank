namespace ArtemisBank.Core.Application.DTOs
{
    public class CreditCardConsumptionDto : BaseDto
    {
        public int CreditCardId { get; set; }
        public decimal Amount { get; set; }
        public required string Commerce { get; set; }
        public required string Status { get; set; }
    }
}