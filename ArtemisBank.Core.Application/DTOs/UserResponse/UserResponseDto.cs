namespace ArtemisBank.Core.Application.DTOs
{
    public class UserResponseDto
    {
        public string? UserId { get; set; }
        public bool HasError { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}