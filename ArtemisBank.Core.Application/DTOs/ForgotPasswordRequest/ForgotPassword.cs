namespace ArtemisBank.Core.Application.DTOs
{
    public class ForgotPasswordRequestDto : BaseDto
    {
        public required string UserName { get; set; }
        public required string Origin { get; set; }
    }
}