using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.Interfaces.Services;
using ArtemisBank.Core.Application.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.WebApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class SavingsAccountController : Controller
    {
        private readonly ISavingsAccountService _savingsAccountService;
        private readonly IAccountServiceForWebApp _accountService;
        private readonly ITransactionService _transactionService;
        private readonly IMapper _mapper;

        public SavingsAccountController(
            ISavingsAccountService savingsAccountService,
            IAccountServiceForWebApp accountService,
            ITransactionService transactionService,
            IMapper mapper)
        {
            _savingsAccountService = savingsAccountService;
            _accountService = accountService;
            _transactionService = transactionService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var accounts = await _savingsAccountService.GetAllList();
            var users = await _accountService.GetAllUsersAsync();

            var page = int.TryParse(Request.Query["page"], out int p) ? p : 1;
            var statusFilter = Request.Query["status"].ToString();
            var typeFilter = Request.Query["type"].ToString();
            var searchIdCard = Request.Query["idcard"].ToString();

            var filteredAccounts = accounts.AsQueryable();

            if (!string.IsNullOrEmpty(searchIdCard))
            {
                var user = users.FirstOrDefault(u => u.IdCard == searchIdCard);
                if (user != null)
                    filteredAccounts = filteredAccounts.Where(a => a.UserId == user.Id);
            }

            if (statusFilter == "activas")
                filteredAccounts = filteredAccounts.Where(a => a.IsActive);
            else if (statusFilter == "canceladas")
                filteredAccounts = filteredAccounts.Where(a => !a.IsActive);

            if (typeFilter == "principal")
                filteredAccounts = filteredAccounts.Where(a => a.IsPrimary);
            else if (typeFilter == "secundaria")
                filteredAccounts = filteredAccounts.Where(a => !a.IsPrimary);

            var accountList = filteredAccounts.OrderByDescending(a => a.CreatedAt).ToList();

            var pageSize = 20;
            var totalPages = (int)Math.Ceiling(accountList.Count / (double)pageSize);
            var pagedAccounts = accountList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var vm = pagedAccounts.Select(a => new SavingsAccountViewModel
            {
                Id = a.Id,
                AccountNumber = a.AccountNumber,
                Balance = a.Balance,
                IsPrimary = a.IsPrimary,
                IsActive = a.IsActive,
                UserId = a.UserId,
                OwnerFullName = users.FirstOrDefault(u => u.Id == a.UserId)?.FirstName + " " +
                                users.FirstOrDefault(u => u.Id == a.UserId)?.LastName ?? ""
            }).ToList();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.TypeFilter = typeFilter;
            ViewBag.SearchIdCard = searchIdCard;

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> AssignAccount()
        {
            var users = await _accountService.GetAllUsersAsync();
            var loans = await GetLoanService().GetAllList();
            var cards = await GetCreditCardService().GetAllList();

            var activeClients = users.Where(u => u.UserType == "Cliente" && u.IsActive).ToList();

            var vm = new AssignSavingsAccountViewModel
            {
                Clients = activeClients.Select(u => new ClientListViewModel
                {
                    Id = u.Id,
                    FullName = $"{u.FirstName} {u.LastName}",
                    IdCard = u.IdCard,
                    Email = u.Email,
                    TotalDebt = loans.Where(l => l.UserId == u.Id).Sum(l => l.Amount) +
                                cards.Where(c => c.UserId == u.Id).Sum(c => c.AmountOwed)
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> SelectClient(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
                return RedirectToAction("AssignAccount");

            var user = await _accountService.GetUserByIdAsync(clientId);
            if (user == null)
                return RedirectToAction("AssignAccount");

            var vm = new SavingsAccountFormViewModel
            {
                ClientId = clientId,
                ClientName = $"{user.FirstName} {user.LastName}"
            };

            return View("AccountForm", vm);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount(SavingsAccountFormViewModel vm)
        {
            if (!ModelState.IsValid)
                return View("AccountForm", vm);

            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var accountNumber = await _savingsAccountService.GenerateUniqueAccountNumberAsync();

            var accountDto = new SavingsAccountDto
            {
                AccountNumber = accountNumber,
                Balance = vm.InitialBalance,
                IsPrimary = false,
                IsActive = true,
                UserId = vm.ClientId,
                CreatedAt = DateTime.Now
            };

            await _savingsAccountService.AddAsync(accountDto);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var account = await _savingsAccountService.GetById(id);
            if (account == null) return RedirectToAction("Index");

            var transactions = await _transactionService.GetAllList();
            account.Transactions = transactions
                .Where(t => t.SavingsAccountId == id)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();

            var users = await _accountService.GetAllUsersAsync();
            account.OwnerFullName = users.FirstOrDefault(u => u.Id == account.UserId)?.FirstName + " " +
                                    users.FirstOrDefault(u => u.Id == account.UserId)?.LastName ?? "";

            return View(account);
        }

        [HttpGet]
        public async Task<IActionResult> Cancel(int id)
        {
            var account = await _savingsAccountService.GetById(id);
            if (account == null) return RedirectToAction("Index");

            if (account.IsPrimary)
            {
                TempData["Error"] = "Las cuentas principales no se pueden cancelar.";
                return RedirectToAction("Index");
            }

            var vm = new CancelSavingsAccountViewModel
            {
                Id = account.Id,
                AccountNumber = account.AccountNumber,
                Balance = account.Balance
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmCancel(CancelSavingsAccountViewModel vm)
        {
            var account = await _savingsAccountService.GetById(vm.Id);
            if (account == null) return RedirectToAction("Index");

            // Si tiene balance, transferirlo a cuenta principal
            if (account.Balance > 0)
            {
                var primaryAccount = await _savingsAccountService.GetPrimaryAccountByUserIdAsync(account.UserId);
                if (primaryAccount != null)
                {
                    await _savingsAccountService.UpdateBalanceAsync(primaryAccount.AccountNumber, account.Balance);
                    await _savingsAccountService.UpdateBalanceAsync(account.AccountNumber, -account.Balance);
                }
            }

            await _savingsAccountService.CancelAsync(vm.Id);
            return RedirectToAction("Index");
        }

        #region Private Methods

        private ILoanService GetLoanService()
        {
            return HttpContext.RequestServices.GetRequiredService<ILoanService>();
        }

        private ICreditCardService GetCreditCardService()
        {
            return HttpContext.RequestServices.GetRequiredService<ICreditCardService>();
        }

        #endregion
    }
}