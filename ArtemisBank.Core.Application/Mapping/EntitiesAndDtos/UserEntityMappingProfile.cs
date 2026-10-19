using AutoMapper;
using ArtemisBank.Core.Application.DTOs;

namespace ArtemisBank.Core.Application.Mapping.EntitiesAndDtos
{
    public class UserEntityMappingProfile : Profile
    {
        public UserEntityMappingProfile()
        {
            CreateMap<UserDto, UserDto>()
                .ReverseMap();
        }
    }
}