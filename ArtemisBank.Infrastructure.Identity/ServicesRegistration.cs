using ArtemisBank.Core.Application.Interfaces.Services;
using ArtemisBank.Core.Domain.Settings;
using ArtemisBank.Infrastructure.Identity.Contexts;
using ArtemisBank.Infrastructure.Identity.Entities;
using ArtemisBank.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArtemisBank.Infrastructure.Identity
{
    public static class ServicesRegistration
    {
        public static void AddIdentityLayerIoc(this IServiceCollection services, IConfiguration config)
        {
            #region Contexts
            var connectionString = config.GetConnectionString("DefaultConnection");
            services.AddDbContext<IdentityContext>(options =>
                options.UseSqlServer(connectionString,
                    m => m.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName)));
            #endregion

            #region Identity
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityContext>()
                .AddDefaultTokenProviders();
            #endregion

            #region Settings
            services.Configure<JwtSettings>(config.GetSection("JwtSettings"));
            #endregion

            #region Services
            services.AddScoped<IAccountServiceForWebApp, AccountServiceForWebApp>();
            services.AddScoped<IAccountServiceForApi, AccountServiceForApi>();
            #endregion
        }
    }
}