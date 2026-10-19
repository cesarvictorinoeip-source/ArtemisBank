using AutoMapper;
using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Application.DTOs;

namespace ArtemisBank.Core.Application.Mapping.EntitiesAndDtos
{
    public class SavingsAccountEntityMappingProfile : Profile
    {
        public SavingsAccountEntityMappingProfile()
        {
            CreateMap<SavingsAccount, SavingsAccountDto>()
                .ForMember(dest => dest.Transactions, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.Transactions, opt => opt.Ignore());
        }
    }
}