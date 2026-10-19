namespace ArtemisBank.Core.Application.DTOs
{
    public class BeneficiaryDto : BaseDto
    {
        public required string UserId { get; set; }
        public required string AccountNumber { get; set; }
        public string? OwnerFullName { get; set; }
    }
}