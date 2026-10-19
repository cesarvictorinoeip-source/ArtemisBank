using ArtemisBank.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBank.Infrastructure.Persistence.EntityConfigurations
{
    public class SavingsAccountConfiguration : IEntityTypeConfiguration<SavingsAccount>
    {
        public void Configure(EntityTypeBuilder<SavingsAccount> builder)
        {
            #region Table
            builder.ToTable("SavingsAccounts");
            builder.HasKey(x => x.Id);
            #endregion

            #region Properties
            builder.Property(x => x.AccountNumber).IsRequired().HasMaxLength(9);
            builder.Property(x => x.Balance).HasPrecision(18, 2);
            builder.Property(x => x.IsPrimary).IsRequired();
            builder.Property(x => x.IsActive).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.UserId).IsRequired();
            #endregion

            #region Relationships
            builder.HasMany(x => x.Transactions)
                .WithOne(x => x.SavingsAccount)
                .HasForeignKey(x => x.SavingsAccountId);
            #endregion
        }
    }
}