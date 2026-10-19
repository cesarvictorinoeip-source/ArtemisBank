using System.ComponentModel.DataAnnotations;
namespace ArtemisBank.Core.Application.ViewModels
{
    public class ThirdPartyTransferViewModel
    {
        [Required(ErrorMessage = "El número de cuenta origen es requerido")]
        public string OriginAccountNumber { get; set; } = "";
        [Required(ErrorMessage = "El monto es requerido")]
        public decimal Amount { get; set; }
        [Required(ErrorMessage = "El número de cuenta destino es requerido")]
        public string DestinationAccountNumber { get; set; } = "";
        public string? DestinationOwnerFullName { get; set; }
        public bool HasError { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}