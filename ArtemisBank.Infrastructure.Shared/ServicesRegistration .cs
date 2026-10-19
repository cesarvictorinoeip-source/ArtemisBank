using ArtemisBank.Core.Application.Interfaces.Services;
using ArtemisBank.Core.Domain.Settings;
using ArtemisBank.Infrastructure.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArtemisBank.Infrastructure.Shared
{
    public static class ServicesRegistration
    {
        public static void AddSharedLayerIoc(this IServiceCollection services, IConfiguration config)
        {
            #region MailSettings
            services.Configure<MailSettings>(config.GetSection("MailSettings"));
            #endregion

            #region Services
            services.AddScoped<IEmailService, EmailService>();
            #endregion
        }
    }
}