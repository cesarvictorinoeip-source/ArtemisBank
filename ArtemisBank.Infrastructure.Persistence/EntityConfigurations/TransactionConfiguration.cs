using ArtemisBank.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBank.Infrastructure.Persistence.EntityConfigurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            #region Table
            builder.ToTable("Transactions");
            builder.HasKey(x => x.Id);
            #endregion

            #region Properties
            builder.Property(x => x.Amount).HasPrecision(18, 2);
            builder.Property(x => x.Type).IsRequired().HasMaxLength(10);
            builder.Property(x => x.Beneficiary).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Origin).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Status).IsRequired().HasMaxLength(20);
            builder.Property(x => x.CreatedAt).IsRequired();
            #endregion
        }
    }
}