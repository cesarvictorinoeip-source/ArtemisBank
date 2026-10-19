using AutoMapper;
using ArtemisBank.Core.Application.Interfaces.Services;
using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using ArtemisBank.Core.Application.DTOs;

namespace ArtemisBank.Core.Application.Services
{
    public class SavingsAccountService : GenericService<SavingsAccount, SavingsAccountDto>, ISavingsAccountService
    {
        private readonly ISavingsAccountRepository _repository;
        private readonly IMapper _mapper;

        public SavingsAccountService(ISavingsAccountRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<SavingsAccountDto?> GetByAccountNumberAsync(string accountNumber)
        {
            var account = await _repository.GetAllQuery()
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
            if (account == null) return null;
            return _mapper.Map<SavingsAccountDto>(account);
        }

        public async Task<List<SavingsAccountDto>> GetByUserIdAsync(string userId)
        {
            var accounts = await _repository.GetAllQuery()
                .Where(a => a.UserId == userId)
                .ToListAsync();
            return _mapper.Map<List<SavingsAccountDto>>(accounts);
        }

        public async Task<SavingsAccountDto?> GetPrimaryAccountByUserIdAsync(string userId)
        {
            var account = await _repository.GetAllQuery()
                .FirstOrDefaultAsync(a => a.UserId == userId && a.IsPrimary);
            if (account == null) return null;
            return _mapper.Map<SavingsAccountDto>(account);
        }

        public async Task UpdateBalanceAsync(string accountNumber, decimal amount)
        {
            var account = await _repository.GetAllQuery()
                .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
            if (account != null)
            {
                account.Balance += amount;
                await _repository.UpdateAsync(account.Id, account);
            }
        }

        public async Task<string> GenerateUniqueAccountNumberAsync()
        {
            var random = new Random();
            string accountNumber;
            bool exists;
            do
            {
                accountNumber = random.Next(100000000, 999999999).ToString();
                exists = await _repository.GetAllQuery()
                    .AnyAsync(a => a.AccountNumber == accountNumber);
            } while (exists);
            return accountNumber;
        }

        public async Task CancelAsync(int id)
        {
            var account = await _repository.GetById(id);
            if (account != null)
            {
                account.IsActive = false;
                await _repository.UpdateAsync(id, account);
            }
        }
    }
}