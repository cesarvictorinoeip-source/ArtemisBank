using AutoMapper;
using ArtemisBank.Core.Application.ViewModels;
using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Application.DTOs;

namespace ArtemisBank.Core.Application.Mapping.DtosAndViewModels
{
    public class CreditCardConsumptionMappingProfile : Profile
    {
        public CreditCardConsumptionMappingProfile()
        {
            CreateMap<CreditCardConsumption, CreditCardConsumptionDto>()
                .ReverseMap();

            CreateMap<CreditCardConsumptionDto, CreditCardConsumptionViewModel>()
                .ReverseMap();
        }
    }
}