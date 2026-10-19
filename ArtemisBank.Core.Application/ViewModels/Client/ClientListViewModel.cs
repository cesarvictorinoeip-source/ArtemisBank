namespace ArtemisBank.Core.Application.ViewModels
{
    public class ClientListViewModel
    {
        public string Id { get; set; } = "";
        public string FullName { get; set; } = "";
        public string IdCard { get; set; } = "";
        public string Email { get; set; } = "";
        public decimal TotalDebt { get; set; }
    }
}