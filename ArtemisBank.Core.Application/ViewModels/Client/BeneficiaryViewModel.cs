namespace ArtemisBank.Core.Application.ViewModels
{
    public class BeneficiaryViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public string AccountNumber { get; set; } = "";
        public string OwnerFullName { get; set; } = "";
    }
}