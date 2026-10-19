using System.ComponentModel.DataAnnotations;

namespace ArtemisBank.Core.Application.DTOs
{
    public class LoanFormApiDto
    {
        [Required]
        public string ClientId { get; set; } = "";
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public decimal InterestRate { get; set; }
        [Required]
        public int TermMonths { get; set; }
    }
}