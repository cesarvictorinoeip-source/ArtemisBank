using AutoMapper;
using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.Interfaces.Services;
using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces.Repositories;

namespace ArtemisBank.Core.Application.Services
{
    public class CommerceService : GenericService<Commerce, CommerceDto>, ICommerceService
    {
        public CommerceService(ICommerceRepository repository, IMapper mapper)
            : base(repository, mapper)
        {
        }
    }
}