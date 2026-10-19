using AutoMapper;
using ArtemisBank.Core.Application.Interfaces.Services;
using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using ArtemisBank.Core.Application.DTOs;

namespace ArtemisBank.Core.Application.Services
{
    public class LoanInstallmentService : GenericService<LoanInstallment, LoanInstallmentDto>, ILoanInstallmentService
    {
        private readonly ILoanInstallmentRepository _repository;
        private readonly IMapper _mapper;

        public LoanInstallmentService(ILoanInstallmentRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<LoanInstallmentDto>> GetByLoanIdAsync(int loanId)
        {
            var installments = await _repository.GetAllQuery()
                .Where(i => i.LoanId == loanId)
                .ToListAsync();
            return _mapper.Map<List<LoanInstallmentDto>>(installments);
        }

        public async Task UpdateLateInstallmentsAsync()
        {
            var installments = await _repository.GetAllList();
            foreach (var installment in installments)
            {
                if (!installment.IsPaid && installment.DueDate < DateTime.UtcNow)
                {
                    installment.IsLate = true;
                    await _repository.UpdateAsync(installment.Id, installment);
                }
            }
        }
    }
}