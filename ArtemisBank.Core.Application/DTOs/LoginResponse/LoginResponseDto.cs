namespace ArtemisBank.Core.Application.DTOs
{
    public class LoginResponseDto
    {
        public string Id { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public bool IsVerified { get; set; }
        public List<string> Roles { get; set; } = [];
        public string? JwtToken { get; set; }
        public bool HasError { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}