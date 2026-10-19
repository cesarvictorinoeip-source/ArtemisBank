using ArtemisBank.Core.Domain.Interfaces.Repositories;
using ArtemisBank.Infrastructure.Persistence.Contexts;
using ArtemisBank.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArtemisBank.Infrastructure.Persistence
{
    public static class ServicesRegistration
    {
        public static void AddPersistenceLayerIoc(this IServiceCollection services, IConfiguration config)
        {
            #region Contexts
            if (config.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddDbContext<ApplicationDbContext>(opt =>
                    opt.UseInMemoryDatabase("AppDb"));
            }
            else
            {
                var connectionString = config.GetConnectionString("DefaultConnection");
                services.AddDbContext<ApplicationDbContext>(
                    (serviceProvider, opt) =>
                    {
                        opt.EnableSensitiveDataLogging();
                        opt.UseSqlServer(connectionString,
                            m => m.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
                    },
                    contextLifetime: ServiceLifetime.Scoped,
                    optionsLifetime: ServiceLifetime.Scoped
                );
            }
            #endregion

            #region Repositories IOC
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<ISavingsAccountRepository, SavingsAccountRepository>();
            services.AddScoped<ILoanRepository, LoanRepository>();
            services.AddScoped<ILoanInstallmentRepository, LoanInstallmentRepository>();
            services.AddScoped<ICreditCardRepository, CreditCardRepository>();
            services.AddScoped<ICreditCardConsumptionRepository, CreditCardConsumptionRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<IBeneficiaryRepository, BeneficiaryRepository>();
            services.AddScoped<ICommerceRepository, CommerceRepository>();
            #endregion
        }
    }
}