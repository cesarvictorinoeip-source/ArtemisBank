using ArtemisBank.Core.Domain.Entities;
using ArtemisBank.Core.Domain.Interfaces.Repositories;
using ArtemisBank.Infrastructure.Persistence.Contexts;

namespace ArtemisBank.Infrastructure.Persistence.Repositories
{
    public class LoanInstallmentRepository : GenericRepository<LoanInstallment>, ILoanInstallmentRepository
    {
        public LoanInstallmentRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}