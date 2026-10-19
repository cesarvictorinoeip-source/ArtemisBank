using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.Interfaces.Services;
using ArtemisBank.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace ArtemisBank.Infrastructure.Identity.Services
{
    public class AccountServiceForWebApp : IAccountServiceForWebApp
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailService _emailService;

        public AccountServiceForWebApp(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        public async Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto)
        {
            LoginResponseDto response = new()
            {
                HasError = false,
                Errors = []
            };

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
                response.Errors.Add($"Su cuenta está inactiva, por favor revise su correo para activarla.");
                return response;
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName ?? "", loginDto.Password, false, true);

            if (!result.Succeeded)
            {
                response.HasError = true;
                if (result.IsLockedOut)
                {
                    response.Errors.Add($"Su cuenta ha sido bloqueada por múltiples intentos fallidos. Intente en 10 minutos.");
                }
                else
                {
                    response.Errors.Add($"Credenciales incorrectas para el usuario: {user.UserName}");
                }
                return response;
            }

            var rolesList = await _userManager.GetRolesAsync(user);

            response.Id = user.Id;
            response.Email = user.Email ?? "";
            response.UserName = user.UserName ?? "";
            response.FirstName = user.FirstName;
            response.LastName = user.LastName;
            response.IsVerified = user.IsActive;
            response.Roles = rolesList.ToList();

            return response;
        }

        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
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
                string verificationUri = await GetVerificationEmailUri(user, origin);
                await _emailService.SendAsync(new EmailRequestDto()
                {
                    To = saveDto.Email,
                    HtmlBody = $"Por favor confirme su cuenta visitando esta URL: {verificationUri}",
                    Subject = "Confirmar registro"
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

        public async Task<UserResponseDto> EditUserAsync(SaveUserDto saveDto, string origin)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };

            var user = await _userManager.FindByIdAsync(saveDto.Id);
            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"No existe una cuenta con este usuario.");
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

            var resetUri = await GetResetPasswordUri(user, request.Origin);
            await _emailService.SendAsync(new EmailRequestDto()
            {
                To = user.Email,
                HtmlBody = $"Restablezca su contraseña visitando esta URL: {resetUri}",
                Subject = "Restablecer contraseña"
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
                response.Errors.Add($"No existe una cuenta con este usuario.");
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

        public async Task<UserResponseDto> ChangeUserStatusAsync(string id, bool status)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"No existe una cuenta con este usuario.");
                return response;
            }

            user.IsActive = status;
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

        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
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
            List<UserDto> listUsersDtos = [];
            var users = await _userManager.Users.ToListAsync();

            foreach (var item in users)
            {
                listUsersDtos.Add(new UserDto
                {
                    Id = item.Id,
                    FirstName = item.FirstName,
                    LastName = item.LastName,
                    IdCard = item.IdCard,
                    Email = item.Email ?? "",
                    UserName = item.UserName ?? "",
                    UserType = item.UserType,
                    IsActive = item.IsActive
                });
            }

            return listUsersDtos;
        }

        public async Task<string> ConfirmAccountAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return "No existe una cuenta con este usuario.";
            }

            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                user.IsActive = true;
                await _userManager.UpdateAsync(user);
                return $"Cuenta confirmada para {user.Email}. Ya puede usar la aplicación.";
            }
            else
            {
                return $"Ocurrió un error al confirmar el correo {user.Email}";
            }
        }

        #region Private methods

        private async Task<string> GetVerificationEmailUri(ApplicationUser user, string origin)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var route = "Login/ConfirmEmail";
            var completeUrl = new Uri(string.Concat(origin, "/", route));
            var verificationUri = QueryHelpers.AddQueryString(completeUrl.ToString(), "userId", user.Id);
            verificationUri = QueryHelpers.AddQueryString(verificationUri, "token", token);
            return verificationUri;
        }

        private async Task<string> GetResetPasswordUri(ApplicationUser user, string origin)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var route = "Login/ResetPassword";
            var completeUrl = new Uri(string.Concat(origin, "/", route));
            var resetUri = QueryHelpers.AddQueryString(completeUrl.ToString(), "userId", user.Id);
            resetUri = QueryHelpers.AddQueryString(resetUri, "token", token);
            return resetUri;
        }

        #endregion
    }
}