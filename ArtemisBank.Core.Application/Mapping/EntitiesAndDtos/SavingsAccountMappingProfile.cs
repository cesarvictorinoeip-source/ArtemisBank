using AutoMapper;
using ArtemisBank.Core.Application.ViewModels;
using ArtemisBank.Core.Application.DTOs;


namespace ArtemisBank.Core.Application.Mapping.DtosAndViewModels
{
    public class SavingsAccountMappingProfile : Profile
    {
        public SavingsAccountMappingProfile()
        {
            CreateMap<SavingsAccountDto, SavingsAccountViewModel>()
                .ReverseMap();
        }
    }
}