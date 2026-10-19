using AutoMapper;
using ArtemisBank.Core.Application.ViewModels;
using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Application.DTOs;

namespace ArtemisBank.Core.Application.Mapping.DtosAndViewModels
{
    public class LoanInstallmentMappingProfile : Profile
    {
        public LoanInstallmentMappingProfile()
        {
            CreateMap<LoanInstallment, LoanInstallmentDto>()
                .ReverseMap();

            CreateMap<LoanInstallmentDto, LoanInstallmentViewModel>()
                .ReverseMap();
        }
    }
}