namespace ArtemisBank.Core.Application.ViewModels
{
    public class SavingsAccountFormViewModel
    {
        public string ClientId { get; set; } = "";
        public string ClientName { get; set; } = "";
        public decimal InitialBalance { get; set; }
        public bool HasError { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}