using AutoMapper;
using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Application.DTOs;

namespace ArtemisBank.Core.Application.Mapping.EntitiesAndDtos
{
    public class CreditCardEntityMappingProfile : Profile
    {
        public CreditCardEntityMappingProfile()
        {
            CreateMap<CreditCard, CreditCardDto>()
                .ForMember(dest => dest.Consumptions, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.Consumptions, opt => opt.Ignore());
        }
    }
}