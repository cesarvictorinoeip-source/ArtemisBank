using AutoMapper;
using ArtemisBank.Core.Application.Interfaces.Services;
using ArtemisBank.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ArtemisBank.Core.Domain.Interfaces.Repositories;
using ArtemisBank.Core.Application.DTOs;

namespace ArtemisBank.Core.Application.Services
{
    public class BeneficiaryService : GenericService<Beneficiary, BeneficiaryDto>, IBeneficiaryService
    {
        private readonly IBeneficiaryRepository _repository;
        private readonly IMapper _mapper;

        public BeneficiaryService(IBeneficiaryRepository repository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<BeneficiaryDto>> GetByUserIdAsync(string userId)
        {
            var beneficiaries = await _repository.GetAllQuery()
                .Where(b => b.UserId == userId)
                .ToListAsync();
            return _mapper.Map<List<BeneficiaryDto>>(beneficiaries);
        }
    }
}