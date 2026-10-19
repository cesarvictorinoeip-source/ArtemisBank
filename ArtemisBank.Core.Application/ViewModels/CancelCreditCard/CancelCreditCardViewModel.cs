namespace ArtemisBank.Core.Application.ViewModels
{
    public class CancelCreditCardViewModel
    {
        public int Id { get; set; }
        public string CardNumber { get; set; } = "";
        public decimal AmountOwed { get; set; }
        public bool HasError { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}