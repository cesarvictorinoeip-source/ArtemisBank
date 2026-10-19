using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces.Repositories;
using ArtemisBank.Infrastructure.Persistence.Contexts;

namespace ArtemisBank.Infrastructure.Persistence.Repositories
{
    public class CreditCardConsumptionRepository : GenericRepository<CreditCardConsumption>, ICreditCardConsumptionRepository
    {
        public CreditCardConsumptionRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}