using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Domain.Entities;

namespace ArtemisBank.Core.Application.Interfaces.Services
{
    public interface ILoanInstallmentService : IGenericService<LoanInstallment, LoanInstallmentDto>
    {
        Task<List<LoanInstallmentDto>> GetByLoanIdAsync(int loanId);
        Task UpdateLateInstallmentsAsync();
    }
}