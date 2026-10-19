using ArtemisBank.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBank.Infrastructure.Persistence.EntityConfigurations
{
    public class LoanConfiguration : IEntityTypeConfiguration<Loan>
    {
        public void Configure(EntityTypeBuilder<Loan> builder)
        {
            #region Table
            builder.ToTable("Loans");
            builder.HasKey(x => x.Id);
            #endregion

            #region Properties
            builder.Property(x => x.LoanNumber).IsRequired().HasMaxLength(9);
            builder.Property(x => x.Amount).HasPrecision(18, 2);
            builder.Property(x => x.InterestRate).HasPrecision(18, 2);
            builder.Property(x => x.TermMonths).IsRequired();
            builder.Property(x => x.IsActive).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.AdminId).IsRequired();
            #endregion

            #region Relationships
            builder.HasMany(x => x.Installments)
                .WithOne(x => x.Loan)
                .HasForeignKey(x => x.LoanId);
            #endregion
        }
    }
}