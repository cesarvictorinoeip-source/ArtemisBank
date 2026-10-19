using ArtemisBank.Infrastructure.Identity.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ArtemisBank.Infrastructure.Identity
{
    public class IdentityContextFactory : IDesignTimeDbContextFactory<IdentityContext>
    {
        public IdentityContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<IdentityContext>();
            optionsBuilder.UseSqlServer("Server=DESKTOP-L2BL2CU;Database=ArtemisBankDb;Trusted_Connection=True;TrustServerCertificate=True;");
            return new IdentityContext(optionsBuilder.Options);
        }
    }
}