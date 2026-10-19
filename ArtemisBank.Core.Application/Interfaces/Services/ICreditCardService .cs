using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Domain.Entities;

namespace ArtemisBank.Core.Application.Interfaces.Services
{
    public interface ICreditCardService : IGenericService<CreditCard, CreditCardDto>
    {
        Task<List<CreditCardDto>> GetByUserIdAsync(string userId);
        Task UpdateLimitAsync(int id, decimal newLimit);
        Task CancelAsync(int id);
    }
}