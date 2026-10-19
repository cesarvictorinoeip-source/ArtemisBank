using ArtemisBank.Core.Domain.Common;

namespace ArtemisBank.Core.Domain.Entities
{
    public class Commerce : BaseEntity
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";
        public bool IsActive { get; set; }
        public string? UserId { get; set; }
    }
}