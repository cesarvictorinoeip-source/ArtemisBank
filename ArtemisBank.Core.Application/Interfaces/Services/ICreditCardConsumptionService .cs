using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Application.DTOs;

namespace ArtemisBank.Core.Application.Interfaces.Services
{
    public interface ICreditCardConsumptionService : IGenericService<CreditCardConsumption, CreditCardConsumptionDto>
    {
        Task<List<CreditCardConsumptionDto>> GetByCardIdAsync(int cardId);
    }
}