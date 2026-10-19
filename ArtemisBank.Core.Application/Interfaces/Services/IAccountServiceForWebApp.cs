using ArtemisBank.Core.Application.DTOs;

namespace ArtemisBank.Core.Application.Interfaces.Services
{
    public interface IAccountServiceForWebApp
    {
        Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto);
        Task SignOutAsync();
        Task<UserResponseDto> RegisterUserAsync(SaveUserDto saveDto, string origin);
        Task<UserResponseDto> EditUserAsync(SaveUserDto saveDto, string origin);
        Task<UserResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request);
        Task<UserResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request);
        Task<UserResponseDto> ChangeUserStatusAsync(string id, bool status);
        Task<UserDto?> GetUserByIdAsync(string id);
        Task<UserDto?> GetUserByUsernameAsync(string username);
        Task<List<UserDto>> GetAllUsersAsync();
        Task<string> ConfirmAccountAsync(string userId, string token);
    }
}