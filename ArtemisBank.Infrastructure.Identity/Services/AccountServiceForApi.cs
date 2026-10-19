using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.Interfaces.Services;
using ArtemisBank.Core.Domain.Settings;
using ArtemisBank.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ArtemisBank.Infrastructure.Identity.Services
{
    public class AccountServiceForApi : IAccountServiceForApi
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly JwtSettings _jwtSettings;

        public AccountServiceForApi(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IOptions<JwtSettings> jwtSettings)
        {
            _userManager = userManager;
            _emailService = emailService;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto)
        {
            LoginResponseDto response = new() { HasError = false, Errors = [] };

            var user = await _userManager.FindByNameAsync(loginDto.UserName);
            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"No existe una cuenta con el usuario: {loginDto.UserName}");
                return response;
            }

            if (!user.IsActive)
            {
                response.HasError = true;
                response.Errors.Add("Su cuenta está inactiva.");
                return response;
            }

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!result)
            {
                response.HasError = true;
                response.Errors.Add("Credenciales incorrectas.");
                return response;
            }

            var rolesList = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, rolesList.ToList());

            response.Id = user.Id;
            response.Email = user.Email ?? "";
            response.UserName = user.UserName ?? "";
            response.FirstName = user.FirstName;
            response.LastName = user.LastName;
            response.IsVerified = user.IsActive;
            response.Roles = rolesList.ToList();
            response.JwtToken = token;

            return response;
        }

        public async Task<UserResponseDto> RegisterUserAsync(SaveUserDto saveDto, string origin)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };

            var userWithSameUserName = await _userManager.FindByNameAsync(saveDto.UserName);
            if (userWithSameUserName != null)
            {
                response.HasError = true;
                response.Errors.Add($"El usuario {saveDto.UserName} ya está en uso.");
                return response;
            }

            var userWithSameEmail = await _userManager.FindByEmailAsync(saveDto.Email);
            if (userWithSameEmail != null)
            {
                response.HasError = true;
                response.Errors.Add($"El correo {saveDto.Email} ya está en uso.");
                return response;
            }

            ApplicationUser user = new()
            {
                FirstName = saveDto.FirstName,
                LastName = saveDto.LastName,
                IdCard = saveDto.IdCard,
                Email = saveDto.Email,
                UserName = saveDto.UserName,
                UserType = saveDto.UserType,
                IsActive = false,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, saveDto.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, saveDto.UserType);
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = saveDto.Email,
                    Subject = "Confirmar registro",
                    HtmlBody = $"Su token de confirmación es: {token}"
                });

                response.UserId = user.Id;
            }
            else
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
            }

            return response;
        }

        public async Task<UserResponseDto> ConfirmAccountAsync(string userId, string token)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add("No existe una cuenta con este usuario.");
                return response;
            }

            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                user.IsActive = true;
                await _userManager.UpdateAsync(user);
            }
            else
            {
                response.HasError = true;
                response.Errors.Add("Error al confirmar la cuenta.");
            }

            return response;
        }

        public async Task<UserResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };

            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"No existe una cuenta con el usuario: {request.UserName}");
                return response;
            }

            user.IsActive = false;
            await _userManager.UpdateAsync(user);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            await _emailService.SendAsync(new EmailRequestDto
            {
                To = user.Email,
                Subject = "Restablecer contraseña",
                HtmlBody = $"Su userId es: {user.Id} y su token de restablecimiento es: {token}"
            });

            return response;
        }

        public async Task<UserResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };

            var user = await _userManager.FindByIdAsync(request.Id);
            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add("No existe una cuenta con este usuario.");
                return response;
            }

            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            var result = await _userManager.ResetPasswordAsync(user, token, request.Password);
            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
                return response;
            }

            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            return response;
        }

        public async Task<UserDto?> GetUserByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IdCard = user.IdCard,
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                UserType = user.UserType,
                IsActive = user.IsActive
            };
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            return users.Select(u => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                IdCard = u.IdCard,
                Email = u.Email ?? "",
                UserName = u.UserName ?? "",
                UserType = u.UserType,
                IsActive = u.IsActive
            }).ToList();
        }

        public async Task<UserResponseDto> EditUserAsync(SaveUserDto saveDto, string origin)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };

            var user = await _userManager.FindByIdAsync(saveDto.Id);
            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add("No existe una cuenta con este usuario.");
                return response;
            }

            user.FirstName = saveDto.FirstName;
            user.LastName = saveDto.LastName;
            user.IdCard = saveDto.IdCard;
            user.Email = saveDto.Email;
            user.UserName = saveDto.UserName;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
            }

            return response;
        }

        public async Task<UserResponseDto> ChangeUserStatusAsync(string id, bool status)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add("No existe una cuenta con este usuario.");
                return response;
            }

            user.IsActive = status;
            await _userManager.UpdateAsync(user);

            return response;
        }

        #region Private Methods
        private string GenerateJwtToken(ApplicationUser user, List<string> roles)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email ?? ""),
                new(ClaimTypes.Name, user.UserName ?? ""),
                new("FirstName", user.FirstName),
                new("LastName", user.LastName)
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiration,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        #endregion
    }
}