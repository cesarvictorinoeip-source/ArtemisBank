using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.Interfaces.Services;
using ArtemisBank.Core.Application.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.WebApp.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class ClientController : Controller
    {
        private readonly IAccountServiceForWebApp _accountService;
        private readonly ISavingsAccountService _savingsAccountService;
        private readonly ILoanService _loanService;
        private readonly ILoanInstallmentService _loanInstallmentService;
        private readonly ICreditCardService _creditCardService;
        private readonly ICreditCardConsumptionService _creditCardConsumptionService;
        private readonly IBeneficiaryService _beneficiaryService;
        private readonly ITransactionService _transactionService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public ClientController(
            IAccountServiceForWebApp accountService,
            ISavingsAccountService savingsAccountService,
            ILoanService loanService,
            ILoanInstallmentService loanInstallmentService,
            ICreditCardService creditCardService,
            ICreditCardConsumptionService creditCardConsumptionService,
            IBeneficiaryService beneficiaryService,
            ITransactionService transactionService,
            IEmailService emailService,
            IMapper mapper)
        {
            _accountService = accountService;
            _savingsAccountService = savingsAccountService;
            _loanService = loanService;
            _loanInstallmentService = loanInstallmentService;
            _creditCardService = creditCardService;
            _creditCardConsumptionService = creditCardConsumptionService;
            _beneficiaryService = beneficiaryService;
            _transactionService = transactionService;
            _emailService = emailService;
            _mapper = mapper;
        }

        // ==================== HOME ====================
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var allInstallments = await _loanInstallmentService.GetAllList();

            var accounts = await _savingsAccountService.GetByUserIdAsync(userId);
            var activeAccounts = accounts.Where(a => a.IsActive).ToList();
            var primaryAccount = activeAccounts.FirstOrDefault(a => a.IsPrimary);
            var secondaryAccounts = activeAccounts.Where(a => !a.IsPrimary).OrderByDescending(a => a.Balance).ToList();
            var orderedAccounts = new List<SavingsAccountDto>();
            if (primaryAccount != null) orderedAccounts.Add(primaryAccount);
            orderedAccounts.AddRange(secondaryAccounts);

            var loans = await _loanService.GetByUserIdAsync(userId);
            var activeLoans = loans.Where(l => l.IsActive).Select(l =>
            {
                var loanInstallments = allInstallments.Where(i => i.LoanId == l.Id).ToList();
                int paidInstallments = loanInstallments.Count(i => i.IsPaid);
                int pendingCount = loanInstallments.Count(i => !i.IsPaid);
                decimal pendingAmount = l.TotalInstallments > 0 ? l.Amount * pendingCount / l.TotalInstallments : 0;
                return new LoanViewModel
                {
                    Id = l.Id,
                    LoanNumber = l.LoanNumber,
                    Amount = l.Amount,
                    InterestRate = l.InterestRate,
                    TermMonths = l.TermMonths,
                    TotalInstallments = l.TotalInstallments,
                    PaidInstallments = paidInstallments,
                    PendingAmount = pendingAmount,
                    IsActive = l.IsActive,
                    IsLate = l.IsLate,
                    UserId = l.UserId,
                    OwnerFullName = ""
                };
            }).ToList();

            var cards = await _creditCardService.GetByUserIdAsync(userId);
            var activeCards = cards.Where(c => c.IsActive).Select(c => new CreditCardViewModel
            {
                Id = c.Id,
                CardNumber = c.CardNumber,
                CreditLimit = c.CreditLimit,
                AmountOwed = c.AmountOwed,
                ExpirationDate = c.ExpirationDate,
                IsActive = c.IsActive,
                UserId = c.UserId,
                OwnerFullName = ""
            }).ToList();

            var vm = new ClientDashboardViewModel
            {
                SavingsAccounts = orderedAccounts.Select(a => new SavingsAccountViewModel
                {
                    Id = a.Id,
                    AccountNumber = a.AccountNumber,
                    Balance = a.Balance,
                    IsPrimary = a.IsPrimary,
                    IsActive = a.IsActive,
                    UserId = a.UserId,
                    OwnerFullName = ""
                }).ToList(),
                Loans = activeLoans,
                CreditCards = activeCards
            };

            return View(vm);
        }

        // ==================== ACCOUNT DETAILS ====================
        public async Task<IActionResult> AccountDetails(int id)
        {
            var account = await _savingsAccountService.GetById(id);
            if (account == null) return RedirectToAction("Index");

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            if (account.UserId != userId) return RedirectToAction("Index");

            var transactions = await _transactionService.GetAllList();
            account.Transactions = transactions
                .Where(t => t.SavingsAccountId == id)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();

            return View(account);
        }

        // ==================== LOAN DETAILS ====================
        public async Task<IActionResult> LoanDetails(int id)
        {
            var loan = await _loanService.GetById(id);
            if (loan == null) return RedirectToAction("Index");

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            if (loan.UserId != userId) return RedirectToAction("Index");

            var installments = await _loanInstallmentService.GetAllList();
            loan.Installments = installments.Where(i => i.LoanId == id).OrderBy(i => i.DueDate).ToList();

            return View(loan);
        }

        // ==================== CARD DETAILS ====================
        public async Task<IActionResult> CardDetails(int id)
        {
            var card = await _creditCardService.GetById(id);
            if (card == null) return RedirectToAction("Index");

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            if (card.UserId != userId) return RedirectToAction("Index");

            var consumptions = await _creditCardConsumptionService.GetAllList();
            card.Consumptions = consumptions
                .Where(c => c.CreditCardId == id)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            return View(card);
        }

        // ==================== BENEFICIARIOS ====================
        public async Task<IActionResult> Beneficiaries()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var beneficiaries = await _beneficiaryService.GetByUserIdAsync(userId);
            var users = await _accountService.GetAllUsersAsync();

            var vm = beneficiaries.Select(b =>
            {
                var account = _savingsAccountService.GetByAccountNumberAsync(b.AccountNumber).Result;
                var owner = account != null ? users.FirstOrDefault(u => u.Id == account.UserId) : null;
                return new BeneficiaryViewModel
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    AccountNumber = b.AccountNumber,
                    OwnerFullName = owner != null ? $"{owner.FirstName} {owner.LastName}" : ""
                };
            }).ToList();

            ViewBag.AddBeneficiary = new AddBeneficiaryViewModel();
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddBeneficiary(AddBeneficiaryViewModel vm)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

            var account = await _savingsAccountService.GetByAccountNumberAsync(vm.AccountNumber);
            if (account == null || !account.IsActive)
            {
                TempData["BeneficiaryError"] = "El número de cuenta ingresado no es válido.";
                return RedirectToAction("Beneficiaries");
            }

            var existing = await _beneficiaryService.GetByUserIdAsync(userId);
            if (existing.Any(b => b.AccountNumber == vm.AccountNumber))
            {
                TempData["BeneficiaryError"] = "Este beneficiario ya está registrado.";
                return RedirectToAction("Beneficiaries");
            }

            await _beneficiaryService.AddAsync(new BeneficiaryDto
            {
                UserId = userId,
                AccountNumber = vm.AccountNumber,
                CreatedAt = DateTime.Now
            });

            return RedirectToAction("Beneficiaries");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBeneficiary(int id)
        {
            await _beneficiaryService.DeleteAsync(id);
            return RedirectToAction("Beneficiaries");
        }

        // ==================== TRANSACCIÓN EXPRESS ====================
        [HttpGet]
        public async Task<IActionResult> ExpressTransaction()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var accounts = await _savingsAccountService.GetByUserIdAsync(userId);
            return View(new ExpressTransactionViewModel
            {
                OriginAccounts = accounts.Where(a => a.IsActive).Select(a => new SavingsAccountViewModel
                {
                    Id = a.Id,
                    AccountNumber = a.AccountNumber,
                    Balance = a.Balance,
                    IsPrimary = a.IsPrimary,
                    IsActive = a.IsActive,
                    UserId = a.UserId,
                    OwnerFullName = ""
                }).ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> ExpressTransaction(ExpressTransactionViewModel vm)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var accounts = await _savingsAccountService.GetByUserIdAsync(userId);
            vm.OriginAccounts = accounts.Where(a => a.IsActive).Select(a => new SavingsAccountViewModel
            {
                Id = a.Id,
                AccountNumber = a.AccountNumber,
                Balance = a.Balance,
                IsPrimary = a.IsPrimary,
                IsActive = a.IsActive,
                UserId = a.UserId,
                OwnerFullName = ""
            }).ToList();

            var destinationAccount = await _savingsAccountService.GetByAccountNumberAsync(vm.DestinationAccountNumber);
            if (destinationAccount == null || !destinationAccount.IsActive)
            {
                vm.HasError = true;
                vm.Errors.Add("El número de cuenta destino ingresado no es válido.");
                return View(vm);
            }

            var originAccount = await _savingsAccountService.GetById(vm.OriginAccountId);
            if (originAccount == null || originAccount.Balance < vm.Amount)
            {
                vm.HasError = true;
                vm.Errors.Add("El monto excede el saldo disponible.");
                return View(vm);
            }

            var destUser = await _accountService.GetUserByIdAsync(destinationAccount.UserId);
            vm.DestinationOwnerFullName = $"{destUser?.FirstName} {destUser?.LastName}";
            TempData["ExpressData"] = System.Text.Json.JsonSerializer.Serialize(vm);
            return RedirectToAction("ConfirmExpressTransaction");
        }

        [HttpGet]
        public IActionResult ConfirmExpressTransaction()
        {
            var json = TempData["ExpressData"]?.ToString();
            if (json == null) return RedirectToAction("ExpressTransaction");
            TempData.Keep("ExpressData");
            return View(System.Text.Json.JsonSerializer.Deserialize<ExpressTransactionViewModel>(json));
        }

        [HttpPost]
        public async Task<IActionResult> ProcessExpressTransaction()
        {
            var json = TempData["ExpressData"]?.ToString();
            if (json == null) return RedirectToAction("Index");
            var vm = System.Text.Json.JsonSerializer.Deserialize<ExpressTransactionViewModel>(json);
            if (vm == null) return RedirectToAction("Index");

            var originAccount = await _savingsAccountService.GetById(vm.OriginAccountId);
            var destinationAccount = await _savingsAccountService.GetByAccountNumberAsync(vm.DestinationAccountNumber);
            if (originAccount == null || destinationAccount == null) return RedirectToAction("Index");

            await _savingsAccountService.UpdateBalanceAsync(originAccount.AccountNumber, -vm.Amount);
            await _savingsAccountService.UpdateBalanceAsync(destinationAccount.AccountNumber, vm.Amount);

            await _transactionService.AddAsync(new TransactionDto
            {
                SavingsAccountId = originAccount.Id,
                Amount = vm.Amount,
                Type = "Debito",
                Beneficiary = destinationAccount.AccountNumber,
                Origin = originAccount.AccountNumber,
                Status = "Aprobada",
                CreatedAt = DateTime.Now
            });
            await _transactionService.AddAsync(new TransactionDto
            {
                SavingsAccountId = destinationAccount.Id,
                Amount = vm.Amount,
                Type = "Credito",
                Beneficiary = destinationAccount.AccountNumber,
                Origin = originAccount.AccountNumber,
                Status = "Aprobada",
                CreatedAt = DateTime.Now
            });

            var originUser = await _accountService.GetUserByIdAsync(originAccount.UserId);
            var destUser = await _accountService.GetUserByIdAsync(destinationAccount.UserId);
            string lastFourDest = destinationAccount.AccountNumber.Substring(destinationAccount.AccountNumber.Length - 4);
            string lastFourOrigin = originAccount.AccountNumber.Substring(originAccount.AccountNumber.Length - 4);

            if (originUser != null)
                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = originUser.Email,
                    Subject = $"Transacción realizada a la cuenta {lastFourDest}",
                    HtmlBody = $"Se ha transferido RD$ {vm.Amount:N2} a la cuenta terminada en {lastFourDest}. Fecha y hora: {DateTime.Now:dd/MM/yyyy HH:mm:ss}."
                });

            if (destUser != null)
                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = destUser.Email,
                    Subject = $"Transacción enviada desde la cuenta {lastFourOrigin}",
                    HtmlBody = $"Ha recibido RD$ {vm.Amount:N2} desde la cuenta terminada en {lastFourOrigin}. Fecha y hora: {DateTime.Now:dd/MM/yyyy HH:mm:ss}."
                });

            return RedirectToAction("Index");
        }

        // ==================== PAGO TARJETA ====================
        [HttpGet]
        public async Task<IActionResult> CardPayment()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var accounts = await _savingsAccountService.GetByUserIdAsync(userId);
            var cards = await _creditCardService.GetByUserIdAsync(userId);

            return View(new CardPaymentClientViewModel
            {
                OriginAccounts = accounts.Where(a => a.IsActive).Select(a => new SavingsAccountViewModel
                {
                    Id = a.Id,
                    AccountNumber = a.AccountNumber,
                    Balance = a.Balance,
                    IsPrimary = a.IsPrimary,
                    IsActive = a.IsActive,
                    UserId = a.UserId,
                    OwnerFullName = ""
                }).ToList(),
                Cards = cards.Where(c => c.IsActive).Select(c => new CreditCardViewModel
                {
                    Id = c.Id,
                    CardNumber = c.CardNumber,
                    CreditLimit = c.CreditLimit,
                    AmountOwed = c.AmountOwed,
                    ExpirationDate = c.ExpirationDate,
                    IsActive = c.IsActive,
                    UserId = c.UserId,
                    OwnerFullName = ""
                }).ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> CardPayment(CardPaymentClientViewModel vm)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var accounts = await _savingsAccountService.GetByUserIdAsync(userId);
            var cards = await _creditCardService.GetByUserIdAsync(userId);

            vm.OriginAccounts = accounts.Where(a => a.IsActive).Select(a => new SavingsAccountViewModel
            {
                Id = a.Id,
                AccountNumber = a.AccountNumber,
                Balance = a.Balance,
                IsPrimary = a.IsPrimary,
                IsActive = a.IsActive,
                UserId = a.UserId,
                OwnerFullName = ""
            }).ToList();
            vm.Cards = cards.Where(c => c.IsActive).Select(c => new CreditCardViewModel
            {
                Id = c.Id,
                CardNumber = c.CardNumber,
                CreditLimit = c.CreditLimit,
                AmountOwed = c.AmountOwed,
                ExpirationDate = c.ExpirationDate,
                IsActive = c.IsActive,
                UserId = c.UserId,
                OwnerFullName = ""
            }).ToList();

            var originAccount = await _savingsAccountService.GetById(vm.OriginAccountId);
            if (originAccount == null || originAccount.Balance < vm.Amount)
            {
                vm.HasError = true;
                vm.Errors.Add("El monto excede el saldo disponible.");
                return View(vm);
            }

            TempData["CardPaymentClientData"] = System.Text.Json.JsonSerializer.Serialize(vm);
            return RedirectToAction("ProcessCardPayment");
        }

        [HttpGet]
        public async Task<IActionResult> ProcessCardPayment()
        {
            var json = TempData["CardPaymentClientData"]?.ToString();
            if (json == null) return RedirectToAction("Index");
            var vm = System.Text.Json.JsonSerializer.Deserialize<CardPaymentClientViewModel>(json);
            if (vm == null) return RedirectToAction("Index");

            var originAccount = await _savingsAccountService.GetById(vm.OriginAccountId);
            var card = await _creditCardService.GetById(vm.CardId);
            if (originAccount == null || card == null) return RedirectToAction("Index");

            decimal actualPayment = Math.Min(vm.Amount, card.AmountOwed);
            await _savingsAccountService.UpdateBalanceAsync(originAccount.AccountNumber, -actualPayment);
            card.AmountOwed -= actualPayment;
            await _creditCardService.UpdateAsync(card.Id, card);

            // CORRECCIÓN: Tipo Debito (sale dinero de la cuenta)
            await _transactionService.AddAsync(new TransactionDto
            {
                SavingsAccountId = originAccount.Id,
                Amount = actualPayment,
                Type = "Debito",
                Beneficiary = card.CardNumber.Substring(card.CardNumber.Length - 4),
                Origin = originAccount.AccountNumber,
                Status = "Aprobada",
                CreatedAt = DateTime.Now
            });

            var user = await _accountService.GetUserByIdAsync(originAccount.UserId);
            if (user != null)
            {
                string lastFour = card.CardNumber.Substring(card.CardNumber.Length - 4);
                string lastFourAccount = originAccount.AccountNumber.Substring(originAccount.AccountNumber.Length - 4);
                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = user.Email,
                    Subject = $"Pago realizado a la tarjeta {lastFour}",
                    HtmlBody = $"Se ha realizado un pago de RD$ {actualPayment:N2} a su tarjeta terminada en {lastFour} desde la cuenta terminada en {lastFourAccount}. Fecha y hora: {DateTime.Now:dd/MM/yyyy HH:mm:ss}."
                });
            }

            return RedirectToAction("Index");
        }

        // ==================== PAGO PRÉSTAMO ====================
        [HttpGet]
        public async Task<IActionResult> LoanPayment()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var accounts = await _savingsAccountService.GetByUserIdAsync(userId);
            var loans = await _loanService.GetByUserIdAsync(userId);
            var allInstallments = await _loanInstallmentService.GetAllList();

            return View(new LoanPaymentClientViewModel
            {
                OriginAccounts = accounts.Where(a => a.IsActive).Select(a => new SavingsAccountViewModel
                {
                    Id = a.Id,
                    AccountNumber = a.AccountNumber,
                    Balance = a.Balance,
                    IsPrimary = a.IsPrimary,
                    IsActive = a.IsActive,
                    UserId = a.UserId,
                    OwnerFullName = ""
                }).ToList(),
                Loans = loans.Where(l => l.IsActive).Select(l =>
                {
                    var loanInstallments = allInstallments.Where(i => i.LoanId == l.Id).ToList();
                    int paidInstallments = loanInstallments.Count(i => i.IsPaid);
                    int pendingCount = loanInstallments.Count(i => !i.IsPaid);
                    decimal pendingAmount = l.TotalInstallments > 0 ? l.Amount * pendingCount / l.TotalInstallments : 0;
                    return new LoanViewModel
                    {
                        Id = l.Id,
                        LoanNumber = l.LoanNumber,
                        Amount = l.Amount,
                        InterestRate = l.InterestRate,
                        TermMonths = l.TermMonths,
                        TotalInstallments = l.TotalInstallments,
                        PaidInstallments = paidInstallments,
                        PendingAmount = pendingAmount,
                        IsActive = l.IsActive,
                        IsLate = l.IsLate,
                        UserId = l.UserId,
                        OwnerFullName = ""
                    };
                }).ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> LoanPayment(LoanPaymentClientViewModel vm)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var accounts = await _savingsAccountService.GetByUserIdAsync(userId);
            var loans = await _loanService.GetByUserIdAsync(userId);
            var allInstallments = await _loanInstallmentService.GetAllList();

            vm.OriginAccounts = accounts.Where(a => a.IsActive).Select(a => new SavingsAccountViewModel
            {
                Id = a.Id,
                AccountNumber = a.AccountNumber,
                Balance = a.Balance,
                IsPrimary = a.IsPrimary,
                IsActive = a.IsActive,
                UserId = a.UserId,
                OwnerFullName = ""
            }).ToList();
            vm.Loans = loans.Where(l => l.IsActive).Select(l =>
            {
                var loanInstallments = allInstallments.Where(i => i.LoanId == l.Id).ToList();
                return new LoanViewModel
                {
                    Id = l.Id,
                    LoanNumber = l.LoanNumber,
                    Amount = l.Amount,
                    InterestRate = l.InterestRate,
                    TermMonths = l.TermMonths,
                    TotalInstallments = l.TotalInstallments,
                    IsActive = l.IsActive,
                    IsLate = l.IsLate,
                    UserId = l.UserId,
                    OwnerFullName = ""
                };
            }).ToList();

            var originAccount = await _savingsAccountService.GetById(vm.OriginAccountId);
            if (originAccount == null || originAccount.Balance < vm.Amount)
            {
                vm.HasError = true;
                vm.Errors.Add("El monto excede el saldo disponible.");
                return View(vm);
            }

            TempData["LoanPaymentClientData"] = System.Text.Json.JsonSerializer.Serialize(vm);
            return RedirectToAction("ProcessLoanPayment");
        }

        [HttpGet]
        public async Task<IActionResult> ProcessLoanPayment()
        {
            var json = TempData["LoanPaymentClientData"]?.ToString();
            if (json == null) return RedirectToAction("Index");
            var vm = System.Text.Json.JsonSerializer.Deserialize<LoanPaymentClientViewModel>(json);
            if (vm == null) return RedirectToAction("Index");

            var originAccount = await _savingsAccountService.GetById(vm.OriginAccountId);
            var loan = await _loanService.GetById(vm.LoanId);
            if (originAccount == null || loan == null) return RedirectToAction("Index");

            var installments = await _loanInstallmentService.GetByLoanIdAsync(loan.Id);
            var pendingInstallments = installments.Where(i => !i.IsPaid).OrderBy(i => i.DueDate).ToList();

            decimal remaining = vm.Amount;
            foreach (var installment in pendingInstallments)
            {
                if (remaining <= 0) break;
                if (remaining >= installment.Amount)
                {
                    remaining -= installment.Amount;
                    installment.IsPaid = true;
                    installment.IsLate = false;
                    await _loanInstallmentService.UpdateAsync(installment.Id, installment);
                }
                else break;
            }

            decimal actualPayment = vm.Amount - remaining;
            await _savingsAccountService.UpdateBalanceAsync(originAccount.AccountNumber, -actualPayment);

            // CORRECCIÓN: Tipo Debito (sale dinero de la cuenta)
            await _transactionService.AddAsync(new TransactionDto
            {
                SavingsAccountId = originAccount.Id,
                Amount = actualPayment,
                Type = "Debito",
                Beneficiary = loan.LoanNumber,
                Origin = originAccount.AccountNumber,
                Status = "Aprobada",
                CreatedAt = DateTime.Now
            });

            var user = await _accountService.GetUserByIdAsync(originAccount.UserId);
            if (user != null)
            {
                string lastFourAccount = originAccount.AccountNumber.Substring(originAccount.AccountNumber.Length - 4);
                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = user.Email,
                    Subject = $"Pago realizado al préstamo {loan.LoanNumber}",
                    HtmlBody = $"Se ha realizado un pago de RD$ {actualPayment:N2} al préstamo {loan.LoanNumber} desde la cuenta terminada en {lastFourAccount}. Fecha y hora: {DateTime.Now:dd/MM/yyyy HH:mm:ss}."
                });
            }

            return RedirectToAction("Index");
        }

        // ==================== TRANSACCIÓN A BENEFICIARIO ====================
        [HttpGet]
        public async Task<IActionResult> BeneficiaryTransaction()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var accounts = await _savingsAccountService.GetByUserIdAsync(userId);
            var beneficiaries = await _beneficiaryService.GetByUserIdAsync(userId);
            var users = await _accountService.GetAllUsersAsync();

            var beneficiaryVms = beneficiaries.Select(b =>
            {
                var account = _savingsAccountService.GetByAccountNumberAsync(b.AccountNumber).Result;
                var owner = account != null ? users.FirstOrDefault(u => u.Id == account.UserId) : null;
                return new BeneficiaryViewModel
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    AccountNumber = b.AccountNumber,
                    OwnerFullName = owner != null ? $"{owner.FirstName} {owner.LastName}" : ""
                };
            }).ToList();

            return View(new BeneficiaryTransactionViewModel
            {
                Beneficiaries = beneficiaryVms,
                OriginAccounts = accounts.Where(a => a.IsActive).Select(a => new SavingsAccountViewModel
                {
                    Id = a.Id,
                    AccountNumber = a.AccountNumber,
                    Balance = a.Balance,
                    IsPrimary = a.IsPrimary,
                    IsActive = a.IsActive,
                    UserId = a.UserId,
                    OwnerFullName = ""
                }).ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> BeneficiaryTransaction(BeneficiaryTransactionViewModel vm)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var accounts = await _savingsAccountService.GetByUserIdAsync(userId);
            var beneficiaries = await _beneficiaryService.GetByUserIdAsync(userId);
            var users = await _accountService.GetAllUsersAsync();

            vm.OriginAccounts = accounts.Where(a => a.IsActive).Select(a => new SavingsAccountViewModel
            {
                Id = a.Id,
                AccountNumber = a.AccountNumber,
                Balance = a.Balance,
                IsPrimary = a.IsPrimary,
                IsActive = a.IsActive,
                UserId = a.UserId,
                OwnerFullName = ""
            }).ToList();
            vm.Beneficiaries = beneficiaries.Select(b =>
            {
                var account = _savingsAccountService.GetByAccountNumberAsync(b.AccountNumber).Result;
                var owner = account != null ? users.FirstOrDefault(u => u.Id == account.UserId) : null;
                return new BeneficiaryViewModel
                {
                    Id = b.Id,
                    UserId = b.UserId,
                    AccountNumber = b.AccountNumber,
                    OwnerFullName = owner != null ? $"{owner.FirstName} {owner.LastName}" : ""
                };
            }).ToList();

            var originAccount = await _savingsAccountService.GetById(vm.OriginAccountId);
            if (originAccount == null || originAccount.Balance < vm.Amount)
            {
                vm.HasError = true;
                vm.Errors.Add("El monto excede el saldo disponible.");
                return View(vm);
            }

            var beneficiary = await _beneficiaryService.GetById(vm.BeneficiaryId);
            if (beneficiary == null)
            {
                vm.HasError = true;
                vm.Errors.Add("El beneficiario seleccionado no es válido.");
                return View(vm);
            }

            var destAccount = await _savingsAccountService.GetByAccountNumberAsync(beneficiary.AccountNumber);
            if (destAccount == null)
            {
                vm.HasError = true;
                vm.Errors.Add("La cuenta del beneficiario no es válida.");
                return View(vm);
            }

            var destUser = await _accountService.GetUserByIdAsync(destAccount.UserId);
            vm.DestinationOwnerFullName = $"{destUser?.FirstName} {destUser?.LastName}";
            vm.DestinationAccountNumber = destAccount.AccountNumber;

            TempData["BeneficiaryTransactionData"] = System.Text.Json.JsonSerializer.Serialize(vm);
            return RedirectToAction("ConfirmBeneficiaryTransaction");
        }

        [HttpGet]
        public IActionResult ConfirmBeneficiaryTransaction()
        {
            var json = TempData["BeneficiaryTransactionData"]?.ToString();
            if (json == null) return RedirectToAction("BeneficiaryTransaction");
            TempData.Keep("BeneficiaryTransactionData");
            return View(System.Text.Json.JsonSerializer.Deserialize<BeneficiaryTransactionViewModel>(json));
        }

        [HttpPost]
        public async Task<IActionResult> ProcessBeneficiaryTransaction()
        {
            var json = TempData["BeneficiaryTransactionData"]?.ToString();
            if (json == null) return RedirectToAction("Index");
            var vm = System.Text.Json.JsonSerializer.Deserialize<BeneficiaryTransactionViewModel>(json);
            if (vm == null) return RedirectToAction("Index");

            var originAccount = await _savingsAccountService.GetById(vm.OriginAccountId);
            var destAccount = await _savingsAccountService.GetByAccountNumberAsync(vm.DestinationAccountNumber!);
            if (originAccount == null || destAccount == null) return RedirectToAction("Index");

            await _savingsAccountService.UpdateBalanceAsync(originAccount.AccountNumber, -vm.Amount);
            await _savingsAccountService.UpdateBalanceAsync(destAccount.AccountNumber, vm.Amount);

            await _transactionService.AddAsync(new TransactionDto
            {
                SavingsAccountId = originAccount.Id,
                Amount = vm.Amount,
                Type = "Debito",
                Beneficiary = destAccount.AccountNumber,
                Origin = originAccount.AccountNumber,
                Status = "Aprobada",
                CreatedAt = DateTime.Now
            });
            await _transactionService.AddAsync(new TransactionDto
            {
                SavingsAccountId = destAccount.Id,
                Amount = vm.Amount,
                Type = "Credito",
                Beneficiary = destAccount.AccountNumber,
                Origin = originAccount.AccountNumber,
                Status = "Aprobada",
                CreatedAt = DateTime.Now
            });

            var originUser = await _accountService.GetUserByIdAsync(originAccount.UserId);
            var destUser = await _accountService.GetUserByIdAsync(destAccount.UserId);
            string lastFourDest = destAccount.AccountNumber.Substring(destAccount.AccountNumber.Length - 4);
            string lastFourOrigin = originAccount.AccountNumber.Substring(originAccount.AccountNumber.Length - 4);

            if (originUser != null)
                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = originUser.Email,
                    Subject = $"Transacción realizada a la cuenta {lastFourDest}",
                    HtmlBody = $"Se ha transferido RD$ {vm.Amount:N2} a la cuenta terminada en {lastFourDest}. Fecha y hora: {DateTime.Now:dd/MM/yyyy HH:mm:ss}."
                });

            if (destUser != null)
                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = destUser.Email,
                    Subject = $"Transacción enviada desde la cuenta {lastFourOrigin}",
                    HtmlBody = $"Ha recibido RD$ {vm.Amount:N2} desde la cuenta terminada en {lastFourOrigin}. Fecha y hora: {DateTime.Now:dd/MM/yyyy HH:mm:ss}."
                });

            return RedirectToAction("Index");
        }

        // ==================== AVANCE DE EFECTIVO ====================
        [HttpGet]
        public async Task<IActionResult> CashAdvance()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var accounts = await _savingsAccountService.GetByUserIdAsync(userId);
            var cards = await _creditCardService.GetByUserIdAsync(userId);

            return View(new CashAdvanceViewModel
            {
                DestinationAccounts = accounts.Where(a => a.IsActive).Select(a => new SavingsAccountViewModel
                {
                    Id = a.Id,
                    AccountNumber = a.AccountNumber,
                    Balance = a.Balance,
                    IsPrimary = a.IsPrimary,
                    IsActive = a.IsActive,
                    UserId = a.UserId,
                    OwnerFullName = ""
                }).ToList(),
                Cards = cards.Where(c => c.IsActive).Select(c => new CreditCardViewModel
                {
                    Id = c.Id,
                    CardNumber = c.CardNumber,
                    CreditLimit = c.CreditLimit,
                    AmountOwed = c.AmountOwed,
                    ExpirationDate = c.ExpirationDate,
                    IsActive = c.IsActive,
                    UserId = c.UserId,
                    OwnerFullName = ""
                }).ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> CashAdvance(CashAdvanceViewModel vm)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var accounts = await _savingsAccountService.GetByUserIdAsync(userId);
            var cards = await _creditCardService.GetByUserIdAsync(userId);

            vm.DestinationAccounts = accounts.Where(a => a.IsActive).Select(a => new SavingsAccountViewModel
            {
                Id = a.Id,
                AccountNumber = a.AccountNumber,
                Balance = a.Balance,
                IsPrimary = a.IsPrimary,
                IsActive = a.IsActive,
                UserId = a.UserId,
                OwnerFullName = ""
            }).ToList();
            vm.Cards = cards.Where(c => c.IsActive).Select(c => new CreditCardViewModel
            {
                Id = c.Id,
                CardNumber = c.CardNumber,
                CreditLimit = c.CreditLimit,
                AmountOwed = c.AmountOwed,
                ExpirationDate = c.ExpirationDate,
                IsActive = c.IsActive,
                UserId = c.UserId,
                OwnerFullName = ""
            }).ToList();

            var card = await _creditCardService.GetById(vm.CardId);
            if (card == null)
            {
                vm.HasError = true;
                vm.Errors.Add("La tarjeta seleccionada no es válida.");
                return View(vm);
            }

            decimal availableCredit = card.CreditLimit - card.AmountOwed;
            if (vm.Amount > availableCredit)
            {
                vm.HasError = true;
                vm.Errors.Add($"El monto excede el crédito disponible. Disponible: RD$ {availableCredit:N2}");
                return View(vm);
            }

            var destAccount = await _savingsAccountService.GetById(vm.DestinationAccountId);
            if (destAccount == null)
            {
                vm.HasError = true;
                vm.Errors.Add("La cuenta destino seleccionada no es válida.");
                return View(vm);
            }

            decimal interest = vm.Amount * 0.0625m;
            decimal totalDebt = vm.Amount + interest;

            await _savingsAccountService.UpdateBalanceAsync(destAccount.AccountNumber, vm.Amount);
            card.AmountOwed += totalDebt;
            await _creditCardService.UpdateAsync(card.Id, card);

            await _transactionService.AddAsync(new TransactionDto
            {
                SavingsAccountId = destAccount.Id,
                Amount = vm.Amount,
                Type = "Credito",
                Beneficiary = destAccount.AccountNumber,
                Origin = card.CardNumber.Substring(card.CardNumber.Length - 4),
                Status = "Aprobada",
                CreatedAt = DateTime.Now
            });

            await _creditCardConsumptionService.AddAsync(new CreditCardConsumptionDto
            {
                CreditCardId = card.Id,
                Amount = totalDebt,
                Commerce = "AVANCE",
                Status = "Aprobado",
                CreatedAt = DateTime.Now
            });

            var user = await _accountService.GetUserByIdAsync(userId);
            if (user != null)
            {
                string lastFourCard = card.CardNumber.Substring(card.CardNumber.Length - 4);
                string lastFourAccount = destAccount.AccountNumber.Substring(destAccount.AccountNumber.Length - 4);
                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = user.Email,
                    Subject = $"Avance de efectivo desde la tarjeta {lastFourCard}",
                    HtmlBody = $"Se ha realizado un avance de RD$ {vm.Amount:N2} desde su tarjeta terminada en {lastFourCard} a la cuenta terminada en {lastFourAccount}. Fecha y hora: {DateTime.Now:dd/MM/yyyy HH:mm:ss}."
                });
            }

            return RedirectToAction("Index");
        }

        // ==================== TRANSFERENCIA ENTRE CUENTAS ====================
        [HttpGet]
        public async Task<IActionResult> TransferBetweenAccounts()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var accounts = await _savingsAccountService.GetByUserIdAsync(userId);

            return View(new TransferBetweenAccountsViewModel
            {
                Accounts = accounts.Where(a => a.IsActive).Select(a => new SavingsAccountViewModel
                {
                    Id = a.Id,
                    AccountNumber = a.AccountNumber,
                    Balance = a.Balance,
                    IsPrimary = a.IsPrimary,
                    IsActive = a.IsActive,
                    UserId = a.UserId,
                    OwnerFullName = ""
                }).ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> TransferBetweenAccounts(TransferBetweenAccountsViewModel vm)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var accounts = await _savingsAccountService.GetByUserIdAsync(userId);

            vm.Accounts = accounts.Where(a => a.IsActive).Select(a => new SavingsAccountViewModel
            {
                Id = a.Id,
                AccountNumber = a.AccountNumber,
                Balance = a.Balance,
                IsPrimary = a.IsPrimary,
                IsActive = a.IsActive,
                UserId = a.UserId,
                OwnerFullName = ""
            }).ToList();

            if (vm.OriginAccountId == vm.DestinationAccountId)
            {
                vm.HasError = true;
                vm.Errors.Add("La cuenta de origen y destino no pueden ser la misma.");
                return View(vm);
            }

            var originAccount = await _savingsAccountService.GetById(vm.OriginAccountId);
            if (originAccount == null || originAccount.Balance < vm.Amount)
            {
                vm.HasError = true;
                vm.Errors.Add("El monto excede el saldo disponible.");
                return View(vm);
            }

            var destAccount = await _savingsAccountService.GetById(vm.DestinationAccountId);
            if (destAccount == null)
            {
                vm.HasError = true;
                vm.Errors.Add("La cuenta destino no es válida.");
                return View(vm);
            }

            await _savingsAccountService.UpdateBalanceAsync(originAccount.AccountNumber, -vm.Amount);
            await _savingsAccountService.UpdateBalanceAsync(destAccount.AccountNumber, vm.Amount);

            await _transactionService.AddAsync(new TransactionDto
            {
                SavingsAccountId = originAccount.Id,
                Amount = vm.Amount,
                Type = "Debito",
                Beneficiary = destAccount.AccountNumber,
                Origin = originAccount.AccountNumber,
                Status = "Aprobada",
                CreatedAt = DateTime.Now
            });
            await _transactionService.AddAsync(new TransactionDto
            {
                SavingsAccountId = destAccount.Id,
                Amount = vm.Amount,
                Type = "Credito",
                Beneficiary = destAccount.AccountNumber,
                Origin = originAccount.AccountNumber,
                Status = "Aprobada",
                CreatedAt = DateTime.Now
            });

            return RedirectToAction("Index");
        }
    }
}