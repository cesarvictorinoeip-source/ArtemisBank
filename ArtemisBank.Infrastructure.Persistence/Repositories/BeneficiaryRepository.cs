using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces.Repositories;
using ArtemisBank.Infrastructure.Persistence.Contexts;

namespace ArtemisBank.Infrastructure.Persistence.Repositories
{
    public class BeneficiaryRepository : GenericRepository<Beneficiary>, IBeneficiaryRepository
    {
        public BeneficiaryRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}