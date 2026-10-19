using ArtemisBank.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBank.Infrastructure.Persistence.EntityConfigurations
{
    public class LoanInstallmentConfiguration : IEntityTypeConfiguration<LoanInstallment>
    {
        public void Configure(EntityTypeBuilder<LoanInstallment> builder)
        {
            #region Table
            builder.ToTable("LoanInstallments");
            builder.HasKey(x => x.Id);
            #endregion

            #region Properties
            builder.Property(x => x.Amount).HasPrecision(18, 2);
            builder.Property(x => x.DueDate).IsRequired();
            builder.Property(x => x.IsPaid).IsRequired();
            builder.Property(x => x.IsLate).IsRequired();
            #endregion
        }
    }
}