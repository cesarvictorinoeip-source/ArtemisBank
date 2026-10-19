using AutoMapper;
using ArtemisBank.Core.Application.ViewModels;
using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Application.DTOs;

namespace ArtemisBank.Core.Application.Mapping.DtosAndViewModels
{
    public class TransactionMappingProfile : Profile
    {
        public TransactionMappingProfile()
        {
            CreateMap<Transaction, TransactionDto>()
                .ReverseMap();

            CreateMap<TransactionDto, TransactionViewModel>()
                .ReverseMap();
        }
    }
}