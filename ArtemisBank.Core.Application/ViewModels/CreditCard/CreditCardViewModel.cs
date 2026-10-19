namespace ArtemisBank.Core.Application.ViewModels
{
    public class CreditCardViewModel
    {
        public int Id { get; set; }
        public required string CardNumber { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal AmountOwed { get; set; }
        public required string ExpirationDate { get; set; }
        public bool IsActive { get; set; }
        public required string UserId { get; set; }
        public required string OwnerFullName { get; set; }
    }
}