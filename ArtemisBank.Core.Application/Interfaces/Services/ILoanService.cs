using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Domain.Entities;

namespace ArtemisBank.Core.Application.Interfaces.Services
{
    public interface ILoanService : IGenericService<Loan, LoanDto>
    {
        Task<List<LoanDto>> GetByUserIdAsync(string userId);
        Task CreateLoanAsync(LoanDto dto);
        Task UpdateInterestRateAsync(int loanId, decimal newRate);
    }
}