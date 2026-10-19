namespace ArtemisBank.Core.Application.ViewModels
{
    public class AssignCreditCardViewModel
    {
        public string SelectedClientId { get; set; } = "";
        public decimal AverageDebt { get; set; }
        public List<ClientListViewModel> Clients { get; set; } = [];
    }
}