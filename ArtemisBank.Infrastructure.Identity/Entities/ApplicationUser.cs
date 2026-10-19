using Microsoft.AspNetCore.Identity;

namespace ArtemisBank.Infrastructure.Identity.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string IdCard { get; set; }
        public bool IsActive { get; set; }
        public required string UserType { get; set; }
    }
}