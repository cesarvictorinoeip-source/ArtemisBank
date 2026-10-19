using ArtemisBank.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBank.Infrastructure.Persistence.EntityConfigurations
{
    public class CreditCardConsumptionConfiguration : IEntityTypeConfiguration<CreditCardConsumption>
    {
        public void Configure(EntityTypeBuilder<CreditCardConsumption> builder)
        {
            #region Table
            builder.ToTable("CreditCardConsumptions");
            builder.HasKey(x => x.Id);
            #endregion

            #region Properties
            builder.Property(x => x.Amount).HasPrecision(18, 2);
            builder.Property(x => x.Commerce).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Status).IsRequired().HasMaxLength(20);
            builder.Property(x => x.CreatedAt).IsRequired();
            #endregion
        }
    }
}