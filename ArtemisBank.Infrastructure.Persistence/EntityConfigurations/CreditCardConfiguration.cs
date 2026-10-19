using ArtemisBank.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBank.Infrastructure.Persistence.EntityConfigurations
{
    public class CreditCardConfiguration : IEntityTypeConfiguration<CreditCard>
    {
        public void Configure(EntityTypeBuilder<CreditCard> builder)
        {
            #region Table
            builder.ToTable("CreditCards");
            builder.HasKey(x => x.Id);
            #endregion

            #region Properties
            builder.Property(x => x.CardNumber).IsRequired().HasMaxLength(16);
            builder.Property(x => x.CVC).IsRequired().HasMaxLength(256);
            builder.Property(x => x.CreditLimit).HasPrecision(18, 2);
            builder.Property(x => x.AmountOwed).HasPrecision(18, 2);
            builder.Property(x => x.ExpirationDate).IsRequired().HasMaxLength(5);
            builder.Property(x => x.IsActive).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.AdminId).IsRequired();
            #endregion

            #region Relationships
            builder.HasMany(x => x.Consumptions)
                .WithOne(x => x.CreditCard)
                .HasForeignKey(x => x.CreditCardId);
            #endregion
        }
    }
}