using ArtemisBank.Core.Application.DTOs;

namespace ArtemisBank.Core.Application.Interfaces.Services
{
    public interface IAccountServiceForApi
    {
        Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto);
        Task<UserResponseDto> RegisterUserAsync(SaveUserDto saveDto, string origin);
        Task<UserResponseDto> ConfirmAccountAsync(string userId, string token);
        Task<UserResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request);
        Task<UserResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request);
        Task<UserResponseDto> EditUserAsync(SaveUserDto saveDto, string origin);
        Task<UserResponseDto> ChangeUserStatusAsync(string id, bool status);
        Task<UserDto?> GetUserByIdAsync(string id);
        Task<List<UserDto>> GetAllUsersAsync();
    }
}