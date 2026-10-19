using AutoMapper;
using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.ViewModels;

namespace ArtemisBank.Core.Application.Mapping.DtosAndViewModels
{
    public class BeneficiaryMappingProfile : Profile
    {
        public BeneficiaryMappingProfile()
        {
            CreateMap<Beneficiary, BeneficiaryDto>()
                .ReverseMap();

            CreateMap<BeneficiaryDto, BeneficiaryViewModel>()
                .ReverseMap();
        }
    }
}