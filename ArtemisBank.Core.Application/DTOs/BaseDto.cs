namespace ArtemisBank.Core.Application.DTOs
{
    public class BaseDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}