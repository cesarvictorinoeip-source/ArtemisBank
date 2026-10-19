using AutoMapper;
using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Domain.Entities;

namespace ArtemisBank.Core.Application.Mapping.DtosAndViewModels
{
    public class CommerceMappingProfile : Profile
    {
        public CommerceMappingProfile()
        {
            CreateMap<Commerce, CommerceDto>().ReverseMap();
        }
    }
}