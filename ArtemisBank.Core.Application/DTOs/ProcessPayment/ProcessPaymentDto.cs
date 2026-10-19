using System.ComponentModel.DataAnnotations;

namespace ArtemisBank.Core.Application.DTOs
{
    public class ProcessPaymentDto
    {
        [Required]
        public string CardNumber { get; set; } = "";
        [Required]
        public string MonthExpirationCard { get; set; } = "";
        [Required]
        public string YearExpirationCard { get; set; } = "";
        [Required]
        public string CVC { get; set; } = "";
        [Required]
        public decimal TransactionAmount { get; set; }
    }
}