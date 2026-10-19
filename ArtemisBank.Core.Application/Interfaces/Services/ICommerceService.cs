using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Domain.Entities;

namespace ArtemisBank.Core.Application.Interfaces.Services
{
    public interface ICommerceService : IGenericService<Commerce, CommerceDto>
    {
    }
}