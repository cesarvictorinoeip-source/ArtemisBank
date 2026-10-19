using System.ComponentModel.DataAnnotations;

namespace ArtemisBank.Core.Application.DTOs
{
    public class UpdateRateDto
    {
        [Required]
        public decimal InterestRate { get; set; }
    }
}