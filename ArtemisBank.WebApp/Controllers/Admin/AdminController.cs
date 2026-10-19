using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.Interfaces.Services;
using ArtemisBank.Core.Application.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.WebApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly IAccountServiceForWebApp _accountService;
        private readonly ITransactionService _transactionService;
        private readonly ILoanService _loanService;
        private readonly ICreditCardService _creditCardService;
        private readonly ISavingsAccountService _savingsAccountService;
        private readonly IMapper _mapper;

        public AdminController(
            IAccountServiceForWebApp accountService,
            ITransactionService transactionService,
            ILoanService loanService,
            ICreditCardService creditCardService,
            ISavingsAccountService savingsAccountService,
            IMapper mapper)
        {
            _accountService = accountService;
            _transactionService = transactionService;
            _loanService = loanService;
            _creditCardService = creditCardService;
            _savingsAccountService = savingsAccountService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _accountService.GetAllUsersAsync();
            var transactions = await _transactionService.GetAllList();
            var loanDtos = await _loanService.GetAllList();
            var creditCardDtos = await _creditCardService.GetAllList();
            var savingsAccountDtos = await _savingsAccountService.GetAllList();

            var today = DateTime.Today;
            var clients = users.Where(u => u.UserType == "Cliente").ToList();
            var activeClients = clients.Where(u => u.IsActive).ToList();

            decimal totalDebt = loanDtos.Where(l => l.IsActive).Sum(l => l.Amount) +
                                creditCardDtos.Where(c => c.IsActive).Sum(c => c.AmountOwed);
            decimal averageDebt = activeClients.Count > 0 ? totalDebt / activeClients.Count : 0;

            var vm = new AdminDashboardViewModel
            {
                TotalTransactions = transactions.Count,
                TodayTransactions = transactions.Count(t => t.CreatedAt.Date == today),
                TodayPayments = transactions.Count(t => t.CreatedAt.Date == today && t.Type == "Credito"),
                TotalPayments = transactions.Count(t => t.Type == "Credito"),
                ActiveClients = activeClients.Count,
                InactiveClients = clients.Count(u => !u.IsActive),
                TotalFinancialProducts = loanDtos.Count + creditCardDtos.Count + savingsAccountDtos.Count,
                TotalLoans = loanDtos.Count(l => l.IsActive),
                TotalCreditCards = creditCardDtos.Count(c => c.IsActive),
                TotalSavingsAccounts = savingsAccountDtos.Count(s => s.IsActive),
                AverageDebtPerClient = averageDebt
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            ModelState.Clear();
            return View(new SaveUserViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(SaveUserViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var origin = $"{Request.Scheme}://{Request.Host}";
            var saveDto = _mapper.Map<SaveUserDto>(vm);
            var response = await _accountService.RegisterUserAsync(saveDto, origin);

            if (response.HasError)
            {
                vm.HasError = true;
                vm.Errors = response.Errors;
                return View(vm);
            }

            // Si es cliente, crear cuenta de ahorro principal
            if (saveDto.UserType == "Cliente" && response.UserId != null)
            {
                var accountNumber = await _savingsAccountService.GenerateUniqueAccountNumberAsync();
                var savingsAccount = new SavingsAccountDto
                {
                    AccountNumber = accountNumber,
                    Balance = vm.InitialAmount,
                    IsPrimary = true,
                    IsActive = true,
                    UserId = response.UserId,
                    CreatedAt = DateTime.Now
                };
                await _savingsAccountService.AddAsync(savingsAccount);
            }

            return RedirectToAction("UserList");
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _accountService.GetUserByIdAsync(id);
            if (user == null)
                return RedirectToAction("UserList");

            var vm = _mapper.Map<SaveUserViewModel>(user);
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(SaveUserViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var origin = $"{Request.Scheme}://{Request.Host}";
            var saveDto = _mapper.Map<SaveUserDto>(vm);
            var response = await _accountService.EditUserAsync(saveDto, origin);

            if (response.HasError)
            {
                vm.HasError = true;
                vm.Errors = response.Errors;
                return View(vm);
            }

            // Si es cliente, agregar monto adicional a cuenta principal
            if (saveDto.UserType == "Cliente" && vm.AdditionalAmount > 0)
            {
                var primaryAccount = await _savingsAccountService.GetPrimaryAccountByUserIdAsync(saveDto.Id);
                if (primaryAccount != null)
                {
                    await _savingsAccountService.UpdateBalanceAsync(primaryAccount.AccountNumber, vm.AdditionalAmount);
                }
            }

            return RedirectToAction("UserList");
        }

        public async Task<IActionResult> ActivateUser(string id)
        {
            await _accountService.ChangeUserStatusAsync(id, true);
            return RedirectToAction("UserList");
        }

        public async Task<IActionResult> DeactivateUser(string id)
        {
            await _accountService.ChangeUserStatusAsync(id, false);
            return RedirectToAction("UserList");
        }

        public async Task<IActionResult> UserList()
        {
            var users = await _accountService.GetAllUsersAsync();
            var vm = _mapper.Map<List<UserViewModel>>(users);
            return View(vm);
        }
    }
}