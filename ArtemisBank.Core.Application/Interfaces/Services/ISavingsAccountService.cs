using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Domain.Entities;

namespace ArtemisBank.Core.Application.Interfaces.Services
{
    public interface ISavingsAccountService : IGenericService<SavingsAccount, SavingsAccountDto>
    {
        Task<SavingsAccountDto?> GetByAccountNumberAsync(string accountNumber);
        Task<List<SavingsAccountDto>> GetByUserIdAsync(string userId);
        Task<SavingsAccountDto?> GetPrimaryAccountByUserIdAsync(string userId);
        Task UpdateBalanceAsync(string accountNumber, decimal amount);
        Task<string> GenerateUniqueAccountNumberAsync();
        Task CancelAsync(int id);
    }
}