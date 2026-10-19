namespace ArtemisBank.Core.Application.ViewModels
{
    public class AssignLoanViewModel
    {
        public string SelectedClientId { get; set; } = "";
        public string SelectedClientName { get; set; } = "";
        public decimal AverageDebt { get; set; }
        public List<ClientListViewModel> Clients { get; set; } = [];
    }
}