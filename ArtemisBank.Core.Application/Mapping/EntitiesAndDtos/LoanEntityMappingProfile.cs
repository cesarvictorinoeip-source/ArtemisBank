using AutoMapper;
using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Application.DTOs;

namespace ArtemisBank.Core.Application.Mapping.EntitiesAndDtos
{
    public class LoanEntityMappingProfile : Profile
    {
        public LoanEntityMappingProfile()
        {
            CreateMap<Loan, LoanDto>()
                .ForMember(dest => dest.Installments, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.Installments, opt => opt.Ignore());
        }
    }
}