using AutoMapper;
using ArtemisBank.Core.Application.Interfaces.Services;
using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using ArtemisBank.Core.Application.DTOs;

namespace ArtemisBank.Core.Application.Services
{
    public class TransactionService : GenericService<Transaction, TransactionDto>, ITransactionService
    {
        private readonly ITransactionRepository _repository;
        private readonly IMapper _mapper;

        public TransactionService(ITransactionRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<TransactionDto>> GetByAccountIdAsync(int accountId)
        {
            var transactions = await _repository.GetAllQuery()
                .Where(t => t.SavingsAccountId == accountId)
                .ToListAsync();
            return _mapper.Map<List<TransactionDto>>(transactions);
        }
    }
}