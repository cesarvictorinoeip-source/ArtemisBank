namespace ArtemisBank.Core.Application.DTOs
{
    public class CommerceDto : BaseDto
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";
        public bool IsActive { get; set; }
        public string? UserId { get; set; }
    }
}