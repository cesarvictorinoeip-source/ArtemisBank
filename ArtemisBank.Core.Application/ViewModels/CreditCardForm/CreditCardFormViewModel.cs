namespace ArtemisBank.Core.Application.ViewModels
{
    public class CreditCardFormViewModel
    {
        public string ClientId { get; set; } = "";
        public string ClientName { get; set; } = "";
        public decimal CreditLimit { get; set; }
        public bool HasError { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}