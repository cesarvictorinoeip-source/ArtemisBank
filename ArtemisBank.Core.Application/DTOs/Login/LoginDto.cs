namespace ArtemisBank.Core.Application.DTOs
{
    public class LoginDto : BaseDto
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }
}