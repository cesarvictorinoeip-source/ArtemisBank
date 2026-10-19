using ArtemisBank.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ArtemisBank.Infrastructure.Persistence.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<SavingsAccount> SavingsAccounts { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<LoanInstallment> LoanInstallments { get; set; }
        public DbSet<CreditCard> CreditCards { get; set; }
        public DbSet<CreditCardConsumption> CreditCardConsumptions { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Beneficiary> Beneficiaries { get; set; }
        public DbSet<Commerce> Commerces { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}