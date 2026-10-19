using AutoMapper;
using ArtemisBank.Core.Application.ViewModels;
using ArtemisBank.Core.Application.DTOs;

namespace ArtemisBank.Core.Application.Mapping.DtosAndViewModels
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<UserDto, UserViewModel>()
                .ReverseMap();
            CreateMap<UserDto, SaveUserViewModel>()
                .ReverseMap();
            CreateMap<SaveUserViewModel, SaveUserDto>()
                .ReverseMap();
        }
    }
}