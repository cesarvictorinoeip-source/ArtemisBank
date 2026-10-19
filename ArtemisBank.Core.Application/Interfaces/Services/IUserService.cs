using ArtemisBank.Core.Application.DTOs;

namespace ArtemisBank.Core.Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(string id);
        Task CreateUserAsync(UserDto dto, string password);
        Task UpdateUserAsync(UserDto dto);
        Task ChangeUserStatusAsync(string id, bool status);
    }
}