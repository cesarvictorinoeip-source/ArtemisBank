using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Domain.Entities;

namespace ArtemisBank.Core.Application.Interfaces.Services
{
    public interface ITransactionService : IGenericService<Transaction, TransactionDto>
    {
        Task<List<TransactionDto>> GetByAccountIdAsync(int accountId);
    }
}