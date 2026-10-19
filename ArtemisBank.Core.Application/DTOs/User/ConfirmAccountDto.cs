using System.ComponentModel.DataAnnotations;

namespace ArtemisBank.Core.Application.DTOs
{
    public class ConfirmAccountDto
    {
        [Required]
        public string UserId { get; set; } = "";
        [Required]
        public string Token { get; set; } = "";
    }
}