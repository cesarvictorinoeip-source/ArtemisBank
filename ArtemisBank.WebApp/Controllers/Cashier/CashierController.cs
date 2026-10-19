using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.Interfaces.Services;
using ArtemisBank.Core.Application.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.WebApp.Controllers
{
    [Authorize(Roles = "Cajero")]
    public class CashierController : Controller
    {
        private readonly IAccountServiceForWebApp _accountService;
        private readonly ISavingsAccountService _savingsAccountService;
        private readonly ICreditCardService _creditCardService;
        private readonly ILoanService _loanService;
        private readonly ILoanInstallmentService _loanInstallmentService;
        private readonly ITransactionService _transactionService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public CashierController(
            IAccountServiceForWebApp accountService,
            ISavingsAccountService savingsAccountService,
            ICreditCardService creditCardService,
            ILoanService loanService,
            ILoanInstallmentService loanInstallmentService,
            ITransactionService transactionService,
            IEmailService emailService,
            IMapper mapper)
        {
            _accountService = accountService;
            _savingsAccountService = savingsAccountService;
            _creditCardService = creditCardService;
            _loanService = loanService;
            _loanInstallmentService = loanInstallmentService;
            _transactionService = transactionService;
            _emailService = emailService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var cashierId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var today = DateTime.Today;
            var transactions = await _transactionService.GetAllList();
            var todayTransactions = transactions
                .Where(t => t.CreatedAt.Date == today && t.CashierId == cashierId)
                .ToList();

            var vm = new CashierDashboardViewModel
            {
                TodayTransactions = todayTransactions.Count,
                TodayPayments = todayTransactions.Count(t => t.Origin != "DEPÓSITO" && t.Beneficiary != "RETIRO" && t.Type == "Debito"),
                TodayDeposits = todayTransactions.Count(t => t.Origin == "DEPÓSITO"),
                TodayWithdrawals = todayTransactions.Count(t => t.Beneficiary == "RETIRO")
            };

            return View(vm);
        }

        // ==================== DEPÓSITO ====================
        [HttpGet]
        public IActionResult Deposit() => View(new DepositViewModel());

        [HttpPost]
        public async Task<IActionResult> Deposit(DepositViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var account = await _savingsAccountService.GetByAccountNumberAsync(vm.AccountNumber);
            if (account == null || !account.IsActive)
            {
                vm.HasError = true;
                vm.Errors.Add("El número de cuenta ingresado no es válido.");
                return View(vm);
            }

            var user = await _accountService.GetUserByIdAsync(account.UserId);
            vm.OwnerFullName = $"{user?.FirstName} {user?.LastName}";
            TempData["DepositData"] = System.Text.Json.JsonSerializer.Serialize(vm);
            return RedirectToAction("ConfirmDeposit");
        }

        [HttpGet]
        public IActionResult ConfirmDeposit()
        {
            var json = TempData["DepositData"]?.ToString();
            if (json == null) return RedirectToAction("Deposit");
            TempData.Keep("DepositData");
            return View(System.Text.Json.JsonSerializer.Deserialize<DepositViewModel>(json));
        }

        [HttpPost]
        public async Task<IActionResult> ProcessDeposit()
        {
            var cashierId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var json = TempData["DepositData"]?.ToString();
            if (json == null) return RedirectToAction("Index");
            var vm = System.Text.Json.JsonSerializer.Deserialize<DepositViewModel>(json);
            if (vm == null) return RedirectToAction("Index");

            var account = await _savingsAccountService.GetByAccountNumberAsync(vm.AccountNumber);
            if (account == null) return RedirectToAction("Index");

            await _savingsAccountService.UpdateBalanceAsync(vm.AccountNumber, vm.Amount);
            await _transactionService.AddAsync(new TransactionDto
            {
                SavingsAccountId = account.Id,
                Amount = vm.Amount,
                Type = "Credito",
                Beneficiary = vm.AccountNumber,
                Origin = "DEPÓSITO",
                Status = "Aprobada",
                CashierId = cashierId,
                CreatedAt = DateTime.Now
            });

            var user = await _accountService.GetUserByIdAsync(account.UserId);
            if (user != null)
            {
                string lastFour = vm.AccountNumber.Substring(vm.AccountNumber.Length - 4);
                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = user.Email,
                    Subject = $"Depósito realizado a su cuenta {lastFour}",
                    HtmlBody = $"Se ha realizado un depósito de RD$ {vm.Amount:N2} a su cuenta. Fecha y hora: {DateTime.Now:dd/MM/yyyy HH:mm:ss}."
                });
            }
            return RedirectToAction("Index");
        }

        // ==================== RETIRO ====================
        [HttpGet]
        public IActionResult Withdrawal() => View(new WithdrawalViewModel());

        [HttpPost]
        public async Task<IActionResult> Withdrawal(WithdrawalViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var account = await _savingsAccountService.GetByAccountNumberAsync(vm.AccountNumber);
            if (account == null || !account.IsActive)
            {
                vm.HasError = true;
                vm.Errors.Add("El número de cuenta ingresado no es válido.");
                return View(vm);
            }

            if (account.Balance < vm.Amount)
            {
                vm.HasError = true;
                vm.Errors.Add("El monto excede el saldo disponible.");
                return View(vm);
            }

            var user = await _accountService.GetUserByIdAsync(account.UserId);
            vm.OwnerFullName = $"{user?.FirstName} {user?.LastName}";
            TempData["WithdrawalData"] = System.Text.Json.JsonSerializer.Serialize(vm);
            return RedirectToAction("ConfirmWithdrawal");
        }

        [HttpGet]
        public IActionResult ConfirmWithdrawal()
        {
            var json = TempData["WithdrawalData"]?.ToString();
            if (json == null) return RedirectToAction("Withdrawal");
            TempData.Keep("WithdrawalData");
            return View(System.Text.Json.JsonSerializer.Deserialize<WithdrawalViewModel>(json));
        }

        [HttpPost]
        public async Task<IActionResult> ProcessWithdrawal()
        {
            var cashierId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var json = TempData["WithdrawalData"]?.ToString();
            if (json == null) return RedirectToAction("Index");
            var vm = System.Text.Json.JsonSerializer.Deserialize<WithdrawalViewModel>(json);
            if (vm == null) return RedirectToAction("Index");

            var account = await _savingsAccountService.GetByAccountNumberAsync(vm.AccountNumber);
            if (account == null) return RedirectToAction("Index");

            await _savingsAccountService.UpdateBalanceAsync(vm.AccountNumber, -vm.Amount);
            await _transactionService.AddAsync(new TransactionDto
            {
                SavingsAccountId = account.Id,
                Amount = vm.Amount,
                Type = "Debito",
                Beneficiary = "RETIRO",
                Origin = vm.AccountNumber,
                Status = "Aprobada",
                CashierId = cashierId,
                CreatedAt = DateTime.Now
            });

            var user = await _accountService.GetUserByIdAsync(account.UserId);
            if (user != null)
            {
                string lastFour = vm.AccountNumber.Substring(vm.AccountNumber.Length - 4);
                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = user.Email,
                    Subject = $"Retiro realizado a su cuenta {lastFour}",
                    HtmlBody = $"Se ha realizado un retiro de RD$ {vm.Amount:N2} de su cuenta. Fecha y hora: {DateTime.Now:dd/MM/yyyy HH:mm:ss}."
                });
            }
            return RedirectToAction("Index");
        }

        // ==================== PAGO TARJETA ====================
        [HttpGet]
        public IActionResult CreditCardPayment() => View(new CreditCardPaymentViewModel());

        [HttpPost]
        public async Task<IActionResult> CreditCardPayment(CreditCardPaymentViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var account = await _savingsAccountService.GetByAccountNumberAsync(vm.AccountNumber);
            if (account == null || !account.IsActive)
            {
                vm.HasError = true;
                vm.Errors.Add("El número de cuenta ingresado no es válido.");
                return View(vm);
            }

            var cards = await _creditCardService.GetAllList();
            var card = cards.FirstOrDefault(c => c.CardNumber == vm.CardNumber && c.IsActive);
            if (card == null)
            {
                vm.HasError = true;
                vm.Errors.Add("El número de tarjeta ingresado no es válido.");
                return View(vm);
            }

            if (account.Balance < vm.Amount)
            {
                vm.HasError = true;
                vm.Errors.Add("El monto excede el saldo disponible.");
                return View(vm);
            }

            var user = await _accountService.GetUserByIdAsync(card.UserId);
            vm.OwnerFullName = $"{user?.FirstName} {user?.LastName}";
            TempData["CardPaymentData"] = System.Text.Json.JsonSerializer.Serialize(vm);
            return RedirectToAction("ConfirmCreditCardPayment");
        }

        [HttpGet]
        public IActionResult ConfirmCreditCardPayment()
        {
            var json = TempData["CardPaymentData"]?.ToString();
            if (json == null) return RedirectToAction("CreditCardPayment");
            TempData.Keep("CardPaymentData");
            return View(System.Text.Json.JsonSerializer.Deserialize<CreditCardPaymentViewModel>(json));
        }

        [HttpPost]
        public async Task<IActionResult> ProcessCreditCardPayment()
        {
            var cashierId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var json = TempData["CardPaymentData"]?.ToString();
            if (json == null) return RedirectToAction("Index");
            var vm = System.Text.Json.JsonSerializer.Deserialize<CreditCardPaymentViewModel>(json);
            if (vm == null) return RedirectToAction("Index");

            var account = await _savingsAccountService.GetByAccountNumberAsync(vm.AccountNumber);
            var cards = await _creditCardService.GetAllList();
            var card = cards.FirstOrDefault(c => c.CardNumber == vm.CardNumber);
            if (account == null || card == null) return RedirectToAction("Index");

            decimal actualPayment = Math.Min(vm.Amount, card.AmountOwed);
            await _savingsAccountService.UpdateBalanceAsync(vm.AccountNumber, -actualPayment);
            card.AmountOwed -= actualPayment;
            await _creditCardService.UpdateAsync(card.Id, card);

            await _transactionService.AddAsync(new TransactionDto
            {
                SavingsAccountId = account.Id,
                Amount = actualPayment,
                Type = "Debito",
                Beneficiary = vm.CardNumber,
                Origin = vm.AccountNumber,
                Status = "Aprobada",
                CashierId = cashierId,
                CreatedAt = DateTime.Now
            });

            var user = await _accountService.GetUserByIdAsync(card.UserId);
            if (user != null)
            {
                string lastFour = vm.CardNumber.Substring(vm.CardNumber.Length - 4);
                string lastFourAccount = vm.AccountNumber.Substring(vm.AccountNumber.Length - 4);
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
        public IActionResult LoanPayment() => View(new LoanPaymentViewModel());

        [HttpPost]
        public async Task<IActionResult> LoanPayment(LoanPaymentViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var account = await _savingsAccountService.GetByAccountNumberAsync(vm.AccountNumber);
            if (account == null || !account.IsActive)
            {
                vm.HasError = true;
                vm.Errors.Add("El número de cuenta ingresado no es válido.");
                return View(vm);
            }

            var loans = await _loanService.GetAllList();
            var loan = loans.FirstOrDefault(l => l.LoanNumber.Trim() == vm.LoanNumber.Trim() && l.IsActive);
            if (loan == null)
            {
                vm.HasError = true;
                vm.Errors.Add("El número de préstamo ingresado no es válido.");
                return View(vm);
            }

            if (account.Balance < vm.Amount)
            {
                vm.HasError = true;
                vm.Errors.Add("El monto excede el saldo disponible.");
                return View(vm);
            }

            var user = await _accountService.GetUserByIdAsync(loan.UserId);
            vm.OwnerFullName = $"{user?.FirstName} {user?.LastName}";
            TempData["LoanPaymentData"] = System.Text.Json.JsonSerializer.Serialize(vm);
            return RedirectToAction("ConfirmLoanPayment");
        }

        [HttpGet]
        public IActionResult ConfirmLoanPayment()
        {
            var json = TempData["LoanPaymentData"]?.ToString();
            if (json == null) return RedirectToAction("LoanPayment");
            TempData.Keep("LoanPaymentData");
            return View(System.Text.Json.JsonSerializer.Deserialize<LoanPaymentViewModel>(json));
        }

        [HttpPost]
        public async Task<IActionResult> ProcessLoanPayment()
        {
            var cashierId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var json = TempData["LoanPaymentData"]?.ToString();
            if (json == null) return RedirectToAction("Index");
            var vm = System.Text.Json.JsonSerializer.Deserialize<LoanPaymentViewModel>(json);
            if (vm == null) return RedirectToAction("Index");

            var account = await _savingsAccountService.GetByAccountNumberAsync(vm.AccountNumber);
            var loans = await _loanService.GetAllList();
            var loan = loans.FirstOrDefault(l => l.LoanNumber == vm.LoanNumber);
            if (account == null || loan == null) return RedirectToAction("Index");

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
            await _savingsAccountService.UpdateBalanceAsync(vm.AccountNumber, -actualPayment);

            await _transactionService.AddAsync(new TransactionDto
            {
                SavingsAccountId = account.Id,
                Amount = actualPayment,
                Type = "Debito",
                Beneficiary = vm.LoanNumber,
                Origin = vm.AccountNumber,
                Status = "Aprobada",
                CashierId = cashierId,
                CreatedAt = DateTime.Now
            });

            var user = await _accountService.GetUserByIdAsync(loan.UserId);
            if (user != null)
            {
                string lastFourAccount = vm.AccountNumber.Substring(vm.AccountNumber.Length - 4);
                await _emailService.SendAsync(new EmailRequestDto
                {
                    To = user.Email,
                    Subject = $"Pago realizado al préstamo {vm.LoanNumber}",
                    HtmlBody = $"Se ha realizado un pago de RD$ {actualPayment:N2} al préstamo {vm.LoanNumber} desde la cuenta terminada en {lastFourAccount}. Fecha y hora: {DateTime.Now:dd/MM/yyyy HH:mm:ss}."
                });
            }
            return RedirectToAction("Index");
        }

        // ==================== TRANSFERENCIA A TERCEROS ====================
        [HttpGet]
        public IActionResult ThirdPartyTransfer() => View(new ThirdPartyTransferViewModel());

        [HttpPost]
        public async Task<IActionResult> ThirdPartyTransfer(ThirdPartyTransferViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var originAccount = await _savingsAccountService.GetByAccountNumberAsync(vm.OriginAccountNumber);
            if (originAccount == null || !originAccount.IsActive)
            {
                vm.HasError = true;
                vm.Errors.Add("El número de cuenta origen ingresado no es válido.");
                return View(vm);
            }

            if (originAccount.Balance < vm.Amount)
            {
                vm.HasError = true;
                vm.Errors.Add("El monto excede el saldo disponible.");
                return View(vm);
            }

            var destinationAccount = await _savingsAccountService.GetByAccountNumberAsync(vm.DestinationAccountNumber);
            if (destinationAccount == null || !destinationAccount.IsActive)
            {
                vm.HasError = true;
                vm.Errors.Add("El número de cuenta destino ingresado no es válido.");
                return View(vm);
            }

            var destUser = await _accountService.GetUserByIdAsync(destinationAccount.UserId);
            vm.DestinationOwnerFullName = $"{destUser?.FirstName} {destUser?.LastName}";
            TempData["TransferData"] = System.Text.Json.JsonSerializer.Serialize(vm);
            return RedirectToAction("ConfirmThirdPartyTransfer");
        }

        [HttpGet]
        public IActionResult ConfirmThirdPartyTransfer()
        {
            var json = TempData["TransferData"]?.ToString();
            if (json == null) return RedirectToAction("ThirdPartyTransfer");
            TempData.Keep("TransferData");
            return View(System.Text.Json.JsonSerializer.Deserialize<ThirdPartyTransferViewModel>(json));
        }

        [HttpPost]
        public async Task<IActionResult> ProcessThirdPartyTransfer()
        {
            var cashierId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
            var json = TempData["TransferData"]?.ToString();
            if (json == null) return RedirectToAction("Index");
            var vm = System.Text.Json.JsonSerializer.Deserialize<ThirdPartyTransferViewModel>(json);
            if (vm == null) return RedirectToAction("Index");

            var originAccount = await _savingsAccountService.GetByAccountNumberAsync(vm.OriginAccountNumber);
            var destinationAccount = await _savingsAccountService.GetByAccountNumberAsync(vm.DestinationAccountNumber);
            if (originAccount == null || destinationAccount == null) return RedirectToAction("Index");

            await _savingsAccountService.UpdateBalanceAsync(vm.OriginAccountNumber, -vm.Amount);
            await _savingsAccountService.UpdateBalanceAsync(vm.DestinationAccountNumber, vm.Amount);

            await _transactionService.AddAsync(new TransactionDto
            {
                SavingsAccountId = originAccount.Id,
                Amount = vm.Amount,
                Type = "Debito",
                Beneficiary = vm.DestinationAccountNumber,
                Origin = vm.OriginAccountNumber,
                Status = "Aprobada",
                CashierId = cashierId,
                CreatedAt = DateTime.Now
            });

            await _transactionService.AddAsync(new TransactionDto
            {
                SavingsAccountId = destinationAccount.Id,
                Amount = vm.Amount,
                Type = "Credito",
                Beneficiary = vm.DestinationAccountNumber,
                Origin = vm.OriginAccountNumber,
                Status = "Aprobada",
                CashierId = cashierId,
                CreatedAt = DateTime.Now
            });

            var originUser = await _accountService.GetUserByIdAsync(originAccount.UserId);
            var destUser = await _accountService.GetUserByIdAsync(destinationAccount.UserId);
            string lastFourDest = vm.DestinationAccountNumber.Substring(vm.DestinationAccountNumber.Length - 4);
            string lastFourOrigin = vm.OriginAccountNumber.Substring(vm.OriginAccountNumber.Length - 4);

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
    }
}