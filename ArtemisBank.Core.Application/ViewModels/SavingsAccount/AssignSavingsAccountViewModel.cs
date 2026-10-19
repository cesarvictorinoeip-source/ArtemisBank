namespace ArtemisBank.Core.Application.ViewModels
{
    public class AssignSavingsAccountViewModel
    {
        public string SelectedClientId { get; set; } = "";
        public List<ClientListViewModel> Clients { get; set; } = [];
    }
}