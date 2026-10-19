using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.Interfaces.Services;
using ArtemisBank.Core.Application.ViewModels;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.WebApp.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class LoanController : Controller
    {
        private readonly ILoanService _loanService;
        private readonly ILoanInstallmentService _loanInstallmentService;
        private readonly IAccountServiceForWebApp _accountService;
        private readonly ISavingsAccountService _savingsAccountService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public LoanController(
            ILoanService loanService,
            ILoanInstallmentService loanInstallmentService,
            IAccountServiceForWebApp accountService,
            ISavingsAccountService savingsAccountService,
            IEmailService emailService,
            IMapper mapper)
        {
            _loanService = loanService;
            _loanInstallmentService = loanInstallmentService;
            _accountService = accountService;
            _savingsAccountService = savingsAccountService;
            _emailService = emailService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            var loans = await _loanService.GetAllList();
            var users = await _accountService.GetAllUsersAsync();
            var allInstallments = await _loanInstallmentService.GetAllList();

            var page = int.TryParse(Request.Query["page"], out int p) ? p : 1;
            var statusFilter = Request.Query["status"].ToString();
            var searchIdCard = Request.Query["idcard"].ToString();

            var filteredLoans = loans.AsQueryable();

            if (!string.IsNullOrEmpty(searchIdCard))
            {
                var user = users.FirstOrDefault(u => u.IdCard == searchIdCard);
                if (user != null)
                    filteredLoans = filteredLoans.Where(l => l.UserId == user.Id);
            }

            if (statusFilter == "activos")
                filteredLoans = filteredLoans.Where(l => l.IsActive);
            else if (statusFilter == "completados")
                filteredLoans = filteredLoans.Where(l => !l.IsActive);

            var loanList = filteredLoans.OrderByDescending(l => l.CreatedAt).ToList();

            var pageSize = 20;
            var totalPages = (int)Math.Ceiling(loanList.Count / (double)pageSize);
            var pagedLoans = loanList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var vm = pagedLoans.Select(l =>
            {
                var loanInstallments = allInstallments.Where(i => i.LoanId == l.Id).ToList();
                int paidInstallments = loanInstallments.Count(i => i.IsPaid);
                int pendingCount = loanInstallments.Count(i => !i.IsPaid);
                decimal pendingAmount = l.TotalInstallments > 0
                    ? l.Amount * pendingCount / l.TotalInstallments
                    : 0;

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
                    OwnerFullName = users.FirstOrDefault(u => u.Id == l.UserId)?.FirstName + " " +
                                    users.FirstOrDefault(u => u.Id == l.UserId)?.LastName ?? ""
                };
            }).ToList();

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.SearchIdCard = searchIdCard;

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> AssignLoan()
        {
            var users = await _accountService.GetAllUsersAsync();
            var loans = await _loanService.GetAllList();
            var creditCards = await GetCreditCardService().GetAllList();

            var activeClients = users.Where(u => u.UserType == "Cliente" && u.IsActive).ToList();
            var clientsWithoutLoan = activeClients
                .Where(u => !loans.Any(l => l.UserId == u.Id && l.IsActive))
                .ToList();

            decimal totalDebt = loans.Where(l => l.IsActive).Sum(l => l.Amount) +
                                creditCards.Where(c => c.IsActive).Sum(c => c.AmountOwed);
            decimal averageDebt = activeClients.Count > 0 ? totalDebt / activeClients.Count : 0;

            var vm = new AssignLoanViewModel
            {
                AverageDebt = averageDebt,
                Clients = clientsWithoutLoan.Select(u => new ClientListViewModel
                {
                    Id = u.Id,
                    FullName = $"{u.FirstName} {u.LastName}",
                    IdCard = u.IdCard,
                    Email = u.Email,
                    TotalDebt = loans.Where(l => l.UserId == u.Id).Sum(l => l.Amount) +
                                creditCards.Where(c => c.UserId == u.Id).Sum(c => c.AmountOwed)
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> SelectClient(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
                return RedirectToAction("AssignLoan");

            var user = await _accountService.GetUserByIdAsync(clientId);
            if (user == null)
                return RedirectToAction("AssignLoan");

            var loans = await _loanService.GetAllList();
            var creditCards = await GetCreditCardService().GetAllList();
            var activeClients = (await _accountService.GetAllUsersAsync())
                .Where(u => u.UserType == "Cliente" && u.IsActive).ToList();

            decimal totalDebt = loans.Where(l => l.IsActive).Sum(l => l.Amount) +
                                creditCards.Where(c => c.IsActive).Sum(c => c.AmountOwed);
            decimal averageDebt = activeClients.Count > 0 ? totalDebt / activeClients.Count : 0;

            var vm = new LoanFormViewModel
            {
                ClientId = clientId,
                ClientName = $"{user.FirstName} {user.LastName}",
                AverageDebt = averageDebt
            };

            return View("LoanForm", vm);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLoan(LoanFormViewModel vm)
        {
            if (!ModelState.IsValid)
                return View("LoanForm", vm);

            var loans = await _loanService.GetAllList();
            var creditCards = await GetCreditCardService().GetAllList();
            var activeClients = (await _accountService.GetAllUsersAsync())
                .Where(u => u.UserType == "Cliente" && u.IsActive).ToList();

            decimal totalDebt = loans.Where(l => l.IsActive).Sum(l => l.Amount) +
                                creditCards.Where(c => c.IsActive).Sum(c => c.AmountOwed);
            decimal averageDebt = activeClients.Count > 0 ? totalDebt / activeClients.Count : 0;

            decimal clientCurrentDebt = loans.Where(l => l.UserId == vm.ClientId).Sum(l => l.Amount) +
                                        creditCards.Where(c => c.UserId == vm.ClientId).Sum(c => c.AmountOwed);

            decimal monthlyRate = vm.InterestRate / 100 / 12;
            decimal totalLoanCost = vm.Amount * (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), vm.TermMonths)) /
                                    ((decimal)Math.Pow((double)(1 + monthlyRate), vm.TermMonths) - 1) * vm.TermMonths;

            bool isHighRisk = clientCurrentDebt > averageDebt ||
                              (clientCurrentDebt + totalLoanCost) > averageDebt;

            if (isHighRisk)
            {
                string message = clientCurrentDebt > averageDebt
                    ? "Este cliente se considera de alto riesgo, ya que su deuda actual supera el promedio del sistema."
                    : "Asignar este préstamo convertirá al cliente en un cliente de alto riesgo, ya que su deuda superará el umbral promedio del sistema.";

                TempData["RiskMessage"] = message;
                TempData["LoanFormData"] = System.Text.Json.JsonSerializer.Serialize(vm);
                return RedirectToAction("RiskWarning");
            }

            await SaveLoan(vm);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult RiskWarning()
        {
            ViewBag.RiskMessage = TempData["RiskMessage"];
            TempData.Keep("LoanFormData");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmRiskyLoan()
        {
            var json = TempData["LoanFormData"]?.ToString();
            if (json == null) return RedirectToAction("Index");

            var vm = System.Text.Json.JsonSerializer.Deserialize<LoanFormViewModel>(json);
            if (vm == null) return RedirectToAction("Index");

            await SaveLoan(vm);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int id)
        {
            var loan = await _loanService.GetById(id);
            if (loan == null) return RedirectToAction("Index");

            var installments = await _loanInstallmentService.GetAllList();
            loan.Installments = installments.Where(i => i.LoanId == id).ToList();

            return View(loan);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var loan = await _loanService.GetById(id);
            if (loan == null) return RedirectToAction("Index");

            var vm = new EditLoanViewModel
            {
                Id = loan.Id,
                LoanNumber = loan.LoanNumber,
                InterestRate = loan.InterestRate
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditLoanViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            await _loanService.UpdateInterestRateAsync(vm.Id, vm.InterestRate);

            var loan = await _loanService.GetById(vm.Id);
            var user = await _accountService.GetUserByIdAsync(loan!.UserId);

            var installments = await _loanInstallmentService.GetAllList();
            var futureInstallments = installments
                .Where(i => i.LoanId == vm.Id && !i.IsPaid && i.DueDate > DateTime.Today)
                .ToList();

            int remainingMonths = futureInstallments.Count;
            if (remainingMonths > 0)
            {
                decimal monthlyRate = vm.InterestRate / 100 / 12;
                decimal pendingAmount = loan.PendingAmount;
                decimal newInstallmentAmount = pendingAmount * (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), remainingMonths)) /
                                              ((decimal)Math.Pow((double)(1 + monthlyRate), remainingMonths) - 1);

                foreach (var installment in futureInstallments)
                {
                    var dto = _mapper.Map<LoanInstallmentDto>(installment);
                    dto.Amount = newInstallmentAmount;
                    await _loanInstallmentService.UpdateAsync(installment.Id, dto);
                }

                if (user != null)
                {
                    await _emailService.SendAsync(new EmailRequestDto
                    {
                        To = user.Email,
                        Subject = "Actualización de tasa de interés",
                        HtmlBody = $"Su tasa de interés ha sido actualizada a {vm.InterestRate}%. " +
                                   $"Su nueva cuota mensual es RD$ {newInstallmentAmount:N2}."
                    });
                }
            }

            return RedirectToAction("Index");
        }

        #region Private Methods

        private ICreditCardService GetCreditCardService()
        {
            return HttpContext.RequestServices.GetRequiredService<ICreditCardService>();
        }

        private async Task SaveLoan(LoanFormViewModel vm)
        {
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

            var random = new Random();
            string loanNumber;
            var allLoans = await _loanService.GetAllList();
            var allAccounts = await _savingsAccountService.GetAllList();
            do
            {
                loanNumber = random.Next(100000000, 999999999).ToString();
            } while (allLoans.Any(l => l.LoanNumber == loanNumber) ||
                     allAccounts.Any(a => a.AccountNumber == loanNumber));

            decimal monthlyRate = vm.InterestRate / 100 / 12;
            decimal installmentAmount = vm.Amount * (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), vm.TermMonths)) /
                                        ((decimal)Math.Pow((double)(1 + monthlyRate), vm.TermMonths) - 1);

            var loanDto = new LoanDto
            {
                LoanNumber = loanNumber,
                Amount = vm.Amount,
                InterestRate = vm.InterestRate,
                TermMonths = vm.TermMonths,
                TotalInstallments = vm.TermMonths,
                PaidInstallments = 0,
                PendingAmount = vm.Amount,
                IsActive = true,
                IsLate = false,
                UserId = vm.ClientId,
                AdminId = adminId,
                CreatedAt = DateTime.Now
            };

            await _loanService.CreateLoanAsync(loanDto);

            var savedLoan = (await _loanService.GetAllList())
                .FirstOrDefault(l => l.LoanNumber == loanNumber);

            if (savedLoan != null)
            {
                for (int i = 1; i <= vm.TermMonths; i++)
                {
                    var installment = new LoanInstallmentDto
                    {
                        LoanId = savedLoan.Id,
                        Amount = installmentAmount,
                        DueDate = DateTime.Now.AddMonths(i),
                        IsPaid = false,
                        IsLate = false,
                        CreatedAt = DateTime.Now
                    };
                    await _loanInstallmentService.AddAsync(installment);
                }

                var primaryAccount = await _savingsAccountService.GetPrimaryAccountByUserIdAsync(vm.ClientId);
                if (primaryAccount != null)
                    await _savingsAccountService.UpdateBalanceAsync(primaryAccount.AccountNumber, vm.Amount);

                var user = await _accountService.GetUserByIdAsync(vm.ClientId);
                if (user != null)
                {
                    await _emailService.SendAsync(new EmailRequestDto
                    {
                        To = user.Email,
                        Subject = "Préstamo aprobado",
                        HtmlBody = $"Su préstamo ha sido aprobado. Monto: RD$ {vm.Amount:N2}, " +
                                   $"Plazo: {vm.TermMonths} meses, Tasa: {vm.InterestRate}%, " +
                                   $"Cuota mensual: RD$ {installmentAmount:N2}."
                    });
                }
            }
        }

        #endregion
    }
}