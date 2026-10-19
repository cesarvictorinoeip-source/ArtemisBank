
namespace ArtemisBank.Core.Application.DTOs
{
    public class SavingsAccountDto : BaseDto
    {
        public required string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsActive { get; set; }
        public required string UserId { get; set; }
        public string? OwnerFullName { get; set; }
        public List<TransactionDto> Transactions { get; set; } = [];
    }
}