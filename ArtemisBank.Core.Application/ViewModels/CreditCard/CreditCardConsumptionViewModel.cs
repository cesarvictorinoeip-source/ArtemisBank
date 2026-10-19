namespace ArtemisBank.Core.Application.ViewModels
{
    public class CreditCardConsumptionViewModel
    {
        public int Id { get; set; }
        public int CreditCardId { get; set; }
        public decimal Amount { get; set; }
        public required string Commerce { get; set; }
        public required string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}