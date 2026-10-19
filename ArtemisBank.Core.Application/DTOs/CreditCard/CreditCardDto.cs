namespace ArtemisBank.Core.Application.DTOs
{
    public class CreditCardDto : BaseDto
    {
        public string CardNumber { get; set; } = "";
        public decimal CreditLimit { get; set; }
        public decimal AmountOwed { get; set; }
        public string ExpirationDate { get; set; } = "";
        public string? CVC { get; set; }
        public bool IsActive { get; set; }
        public string UserId { get; set; } = "";
        public string? AdminId { get; set; }
        public string? OwnerFullName { get; set; }
        public List<CreditCardConsumptionDto> Consumptions { get; set; } = [];
    }
}