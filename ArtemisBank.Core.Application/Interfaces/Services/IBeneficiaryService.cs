using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Domain.Entities;

namespace ArtemisBank.Core.Application.Interfaces.Services
{
    public interface IBeneficiaryService : IGenericService<Beneficiary, BeneficiaryDto>
    {
        Task<List<BeneficiaryDto>> GetByUserIdAsync(string userId);
    }
}