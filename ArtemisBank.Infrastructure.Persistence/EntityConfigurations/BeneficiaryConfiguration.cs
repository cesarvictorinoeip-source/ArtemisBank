using ArtemisBank.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBank.Infrastructure.Persistence.EntityConfigurations
{
    public class BeneficiaryConfiguration : IEntityTypeConfiguration<Beneficiary>
    {
        public void Configure(EntityTypeBuilder<Beneficiary> builder)
        {
            #region Table
            builder.ToTable("Beneficiaries");
            builder.HasKey(x => x.Id);
            #endregion

            #region Properties
            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.AccountNumber).IsRequired().HasMaxLength(9);
            builder.Property(x => x.CreatedAt).IsRequired();
            #endregion
        }
    }
}