using AutoMapper;
using ArtemisBank.Core.Application.Interfaces.Services;
using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using ArtemisBank.Core.Application.DTOs;

namespace ArtemisBank.Core.Application.Services
{
    public class LoanService : GenericService<Loan, LoanDto>, ILoanService
    {
        private readonly ILoanRepository _repository;
        private readonly IMapper _mapper;

        public LoanService(ILoanRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<LoanDto>> GetByUserIdAsync(string userId)
        {
            var loans = await _repository.GetAllQuery()
                .Where(l => l.UserId == userId)
                .ToListAsync();
            return _mapper.Map<List<LoanDto>>(loans);
        }

        public async Task CreateLoanAsync(LoanDto dto)
        {
            var loan = _mapper.Map<Loan>(dto);
            loan.IsActive = true;
            loan.CreatedAt = DateTime.UtcNow;
            await _repository.AddAsync(loan);
        }

        public async Task UpdateInterestRateAsync(int loanId, decimal newRate)
        {
            var loan = await _repository.GetById(loanId);
            if (loan != null)
            {
                loan.InterestRate = newRate;
                await _repository.UpdateAsync(loanId, loan);
            }
        }
    }
}