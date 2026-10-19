namespace ArtemisBank.Core.Application.DTOs
{
    public class EmailRequestDto : BaseDto
    {
        public string? To { get; set; }
        public List<string>? ToRange { get; set; }
        public required string Subject { get; set; }
        public required string HtmlBody { get; set; }


    }
}