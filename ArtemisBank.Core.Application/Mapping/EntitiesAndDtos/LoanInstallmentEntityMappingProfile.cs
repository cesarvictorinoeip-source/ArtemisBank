using AutoMapper;
using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Application.DTOs;

namespace ArtemisBank.Core.Application.Mapping.EntitiesAndDtos
{
    public class LoanInstallmentEntityMappingProfile : Profile
    {
        public LoanInstallmentEntityMappingProfile()
        {
            CreateMap<LoanInstallment, LoanInstallmentDto>()
                .ReverseMap();
        }
    }
}