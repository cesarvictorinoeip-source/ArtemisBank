using ArtemisBank.Core.Application.Interfaces.Services;
using ArtemisBank.Core.Application.Services;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ArtemisBank.Core.Application
{
    public static class ServicesRegistration
    {
        public static void AddApplicationLayerIoc(this IServiceCollection services)
        {
            #region AutoMapper
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddMaps(Assembly.GetExecutingAssembly());
            });
            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
            #endregion

            #region Services IOC
            services.AddScoped(typeof(IGenericService<,>), typeof(GenericService<,>));
            services.AddScoped<ISavingsAccountService, SavingsAccountService>();
            services.AddScoped<ILoanService, LoanService>();
            services.AddScoped<ILoanInstallmentService, LoanInstallmentService>();
            services.AddScoped<ICreditCardService, CreditCardService>();
            services.AddScoped<ICreditCardConsumptionService, CreditCardConsumptionService>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IBeneficiaryService, BeneficiaryService>();
            #endregion
        }
    }
}