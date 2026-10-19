namespace ArtemisBank.Core.Application.ViewModels
{
    public class EditCreditCardViewModel
    {
        public int Id { get; set; }
        public string CardNumber { get; set; } = "";
        public decimal CreditLimit { get; set; }
        public decimal AmountOwed { get; set; }
        public bool HasError { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}