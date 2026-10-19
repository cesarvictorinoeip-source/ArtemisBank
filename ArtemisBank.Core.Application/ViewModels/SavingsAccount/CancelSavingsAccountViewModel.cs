namespace ArtemisBank.Core.Application.ViewModels
{
    public class CancelSavingsAccountViewModel
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = "";
        public decimal Balance { get; set; }
        public bool HasError { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}