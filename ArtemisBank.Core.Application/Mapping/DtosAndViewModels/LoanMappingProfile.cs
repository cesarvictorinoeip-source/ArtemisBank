using AutoMapper;
using ArtemisBank.Core.Application.ViewModels;
using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Application.DTOs;

namespace ArtemisBank.Core.Application.Mapping.DtosAndViewModels
{
    public class LoanMappingProfile : Profile
    {
        public LoanMappingProfile()
        {
            CreateMap<Loan, LoanDto>()
                .ForMember(dest => dest.Installments, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.Installments, opt => opt.Ignore());

            CreateMap<LoanDto, LoanViewModel>()
                .ReverseMap();
        }
    }
}