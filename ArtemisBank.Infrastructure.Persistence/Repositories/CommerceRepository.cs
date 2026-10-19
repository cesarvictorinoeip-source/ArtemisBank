using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces.Repositories;
using ArtemisBank.Infrastructure.Persistence.Contexts;

namespace ArtemisBank.Infrastructure.Persistence.Repositories
{
    public class CommerceRepository : GenericRepository<Commerce>, ICommerceRepository
    {
        public CommerceRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}