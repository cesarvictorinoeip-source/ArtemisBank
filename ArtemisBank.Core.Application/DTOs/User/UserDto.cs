namespace ArtemisBank.Core.Application.DTOs
{
    public class UserDto 
    {
        public string Id { get; set; } = "";
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string IdCard { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public required string UserType { get; set; }
        public bool IsActive { get; set; }
    }
}