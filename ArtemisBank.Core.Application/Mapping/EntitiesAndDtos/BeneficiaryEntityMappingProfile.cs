using AutoMapper;
using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Application.DTOs;

namespace ArtemisBank.Core.Application.Mapping.EntitiesAndDtos
{
    public class BeneficiaryEntityMappingProfile : Profile
    {
        public BeneficiaryEntityMappingProfile()
        {
            CreateMap<Beneficiary, BeneficiaryDto>()
                .ReverseMap();
        }
    }
}