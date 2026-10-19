using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.Interfaces.Services;
using ArtemisBank.Core.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.WebApp.Controllers
{
    public class LoginController : Controller
    {
        private readonly IAccountServiceForWebApp _accountService;

        public LoginController(IAccountServiceForWebApp accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Administrador"))
                    return RedirectToAction("Index", "Admin");
                else if (User.IsInRole("Cajero"))
                    return RedirectToAction("Index", "Cashier");
                else
                    return RedirectToAction("Index", "Client");
            }
            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var response = await _accountService.AuthenticateAsync(new LoginDto
            {
                UserName = vm.UserName,
                Password = vm.Password
            });

            if (response.HasError)
            {
                vm.HasError = true;
                vm.Errors = response.Errors;
                return View(vm);
            }

            if (response.Roles.Contains("Administrador"))
                return RedirectToAction("Index", "Admin");
            else if (response.Roles.Contains("Cajero"))
                return RedirectToAction("Index", "Cashier");
            else
                return RedirectToAction("Index", "Client");
        }

        public async Task<IActionResult> LogOut()
        {
            await _accountService.SignOutAsync();
            return RedirectToAction("Index", "Login");
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var result = await _accountService.ConfirmAccountAsync(userId, token);
            return View("ConfirmEmail", result);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var origin = $"{Request.Scheme}://{Request.Host}";
            var response = await _accountService.ForgotPasswordAsync(new ForgotPasswordRequestDto
            {
                UserName = vm.UserName,
                Origin = origin
            });

            if (response.HasError)
            {
                vm.HasError = true;
                vm.Errors = response.Errors;
                return View(vm);
            }

            TempData["Success"] = "Correo enviado exitosamente";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ResetPassword(string userId, string token)
        {
            return View(new ResetPasswordViewModel { Id = userId, Token = token });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var response = await _accountService.ResetPasswordAsync(new ResetPasswordRequestDto
            {
                Id = vm.Id,
                Token = vm.Token,
                Password = vm.Password
            });

            if (response.HasError)
            {
                vm.HasError = true;
                vm.Errors = response.Errors;
                return View(vm);
            }

            return RedirectToAction("Index");
        }
    }
}