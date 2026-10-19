using AutoMapper;
using ArtemisBank.Core.Application.ViewModels;
using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Application.DTOs;

namespace ArtemisBank.Core.Application.Mapping.DtosAndViewModels
{
    public class CreditCardMappingProfile : Profile
    {
        public CreditCardMappingProfile()
        {
            CreateMap<CreditCard, CreditCardDto>()
                .ForMember(dest => dest.Consumptions, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.Consumptions, opt => opt.Ignore());

            CreateMap<CreditCardDto, CreditCardViewModel>()
                .ReverseMap();
        }
    }
}