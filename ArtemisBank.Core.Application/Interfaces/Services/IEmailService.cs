using ArtemisBank.Core.Application.DTOs;

namespace ArtemisBank.Core.Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendAsync(EmailRequestDto emailRequestDto);
    }
}