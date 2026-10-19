using AutoMapper;
using ArtemisBank.Core.Application.Interfaces.Services;
using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using ArtemisBank.Core.Application.DTOs;

namespace ArtemisBank.Core.Application.Services
{
    public class CreditCardService : GenericService<CreditCard, CreditCardDto>, ICreditCardService
    {
        private readonly ICreditCardRepository _repository;
        private readonly IMapper _mapper;

        public CreditCardService(ICreditCardRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<CreditCardDto>> GetByUserIdAsync(string userId)
        {
            var cards = await _repository.GetAllQuery()
                .Where(c => c.UserId == userId)
                .ToListAsync();
            return _mapper.Map<List<CreditCardDto>>(cards);
        }

        public async Task UpdateLimitAsync(int id, decimal newLimit)
        {
            var card = await _repository.GetById(id);
            if (card != null)
            {
                card.CreditLimit = newLimit;
                await _repository.UpdateAsync(id, card);
            }
        }

        public async Task CancelAsync(int id)
        {
            var card = await _repository.GetById(id);
            if (card != null)
            {
                card.IsActive = false;
                await _repository.UpdateAsync(id, card);
            }
        }
    }
}