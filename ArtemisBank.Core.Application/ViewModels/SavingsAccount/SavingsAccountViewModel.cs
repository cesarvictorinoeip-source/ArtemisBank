namespace ArtemisBank.Core.Application.ViewModels
{
    public class SavingsAccountViewModel
    {
        public int Id { get; set; }
        public required string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
        public required string UserId { get; set; }
        public required string OwnerFullName { get; set; }
    }
}