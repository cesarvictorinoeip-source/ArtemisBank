using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces.Repositories;
using ArtemisBank.Infrastructure.Persistence.Contexts;

namespace ArtemisBank.Infrastructure.Persistence.Repositories
{
    public class SavingsAccountRepository : GenericRepository<SavingsAccount>, ISavingsAccountRepository
    {
        public SavingsAccountRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}