using AutoMapper;
using ArtemisBank.Core.Application.Interfaces.Services;
using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using ArtemisBank.Core.Application.DTOs;

namespace ArtemisBank.Core.Application.Services
{
    public class CreditCardConsumptionService : GenericService<CreditCardConsumption, CreditCardConsumptionDto>, ICreditCardConsumptionService
    {
        private readonly ICreditCardConsumptionRepository _repository;
        private readonly IMapper _mapper;

        public CreditCardConsumptionService(ICreditCardConsumptionRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<CreditCardConsumptionDto>> GetByCardIdAsync(int cardId)
        {
            var consumptions = await _repository.GetAllQuery()
                .Where(c => c.CreditCardId == cardId)
                .ToListAsync();
            return _mapper.Map<List<CreditCardConsumptionDto>>(consumptions);
        }
    }
}