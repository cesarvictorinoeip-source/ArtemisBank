using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.Interfaces.Services;
using ArtemisBank.Core.Domain.Settings;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ArtemisBank.Infrastructure.Shared.Services
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings _mailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<MailSettings> mailSettings, ILogger<EmailService> logger)
        {
            _mailSettings = mailSettings.Value;
            _logger = logger;
        }

        public async Task SendAsync(EmailRequestDto emailRequestDto)
        {
            try
            {
                MimeMessage email = new()
                {
                    Sender = MailboxAddress.Parse(_mailSettings.EmailFrom),
                    Subject = emailRequestDto.Subject
                };

                var recipients = emailRequestDto.ToRange ?? [];
                if (!string.IsNullOrEmpty(emailRequestDto.To))
                    recipients.Add(emailRequestDto.To);

                foreach (var toItem in recipients)
                {
                    email.To.Add(MailboxAddress.Parse(toItem));
                }

                BodyBuilder builder = new()
                {
                    HtmlBody = emailRequestDto.HtmlBody
                };
                email.Body = builder.ToMessageBody();

                using MailKit.Net.Smtp.SmtpClient smtpClient = new();
                smtpClient.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await smtpClient.ConnectAsync(_mailSettings.SmtpHost, _mailSettings.SmtpPort, SecureSocketOptions.Auto);
                await smtpClient.AuthenticateAsync(_mailSettings.SmtpUser, _mailSettings.SmtpPass);
                await smtpClient.SendAsync(email);
                await smtpClient.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occured {Exception}.", ex);
                throw;
            }
        }
    }
}