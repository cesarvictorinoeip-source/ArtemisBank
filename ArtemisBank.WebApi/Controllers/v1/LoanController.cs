using Asp.Versioning;
using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize(Roles = "Administrador")]
    public class LoanController : BaseApiController
    {
        private readonly ILoanService _loanService;
        private readonly ILoanInstallmentService _loanInstallmentService;
        private readonly IAccountServiceForApi _accountService;
        private readonly ISavingsAccountService _savingsAccountService;
        private readonly IEmailService _emailService;

        public LoanController(
            ILoanService loanService,
            ILoanInstallmentService loanInstallmentService,
            IAccountServiceForApi accountService,
            ISavingsAccountService savingsAccountService,
            IEmailService emailService)
        {
            _loanService = loanService;
            _loanInstallmentService = loanInstallmentService;
            _accountService = accountService;
            _savingsAccountService = savingsAccountService;
            _emailService = emailService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? estado = null, [FromQuery] string? cedula = null)
        {
            try
            {
                var loans = await _loanService.GetAllList();
                var users = await _accountService.GetAllUsersAsync();
                var allInstallments = await _loanInstallmentService.GetAllList();

                var filtered = loans.AsQueryable();

                if (!string.IsNullOrEmpty(cedula))
                {
                    var user = users.FirstOrDefault(u => u.IdCard == cedula);
                    if (user != null)
                        filtered = filtered.Where(l => l.UserId == user.Id);
                }

                if (estado == "activos")
                    filtered = filtered.Where(l => l.IsActive);
                else if (estado == "completados")
                    filtered = filtered.Where(l => !l.IsActive);

                var result = filtered.OrderByDescending(l => l.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(l => new
                    {
                        l.Id,
                        l.LoanNumber,
                        l.Amount,
                        l.InterestRate,
                        l.TermMonths,
                        TotalInstallments = l.TotalInstallments,
                        PaidInstallments = allInstallments.Count(i => i.LoanId == l.Id && i.IsPaid),
                        PendingAmount = l.TotalInstallments > 0 ? l.Amount * allInstallments.Count(i => i.LoanId == l.Id && !i.IsPaid) / l.TotalInstallments : 0,
                        l.IsActive,
                        l.IsLate,
                        l.UserId,
                        OwnerFullName = (users.FirstOrDefault(u => u.Id == l.UserId) != null
                        ? users.FirstOrDefault(u => u.Id == l.UserId)!.FirstName + " " +
                          users.FirstOrDefault(u => u.Id == l.UserId)!.LastName
                        : "")
                    }).ToList();

                if (result.Count == 0)
                    return NoContent();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var loan = await _loanService.GetById(id);
                if (loan == null)
                    return NotFound(new { message = "Préstamo no encontrado." });

                var installments = await _loanInstallmentService.GetByLoanIdAsync(id);
                loan.Installments = installments;

                return Ok(loan);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] LoanFormApiDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var loans = await _loanService.GetAllList();
                var hasActiveLoan = loans.Any(l => l.UserId == dto.ClientId && l.IsActive);
                if (hasActiveLoan)
                    return BadRequest(new { message = "El cliente ya tiene un préstamo activo." });

                var cards = await GetCreditCardService().GetAllList();
                var activeClients = (await _accountService.GetAllUsersAsync())
                    .Where(u => u.UserType == "Cliente" && u.IsActive).ToList();

                decimal totalDebt = loans.Where(l => l.IsActive).Sum(l => l.Amount) +
                                    cards.Where(c => c.IsActive).Sum(c => c.AmountOwed);
                decimal averageDebt = activeClients.Count > 0 ? totalDebt / activeClients.Count : 0;

                decimal clientDebt = loans.Where(l => l.UserId == dto.ClientId).Sum(l => l.Amount) +
                                     cards.Where(c => c.UserId == dto.ClientId).Sum(c => c.AmountOwed);

                decimal monthlyRate = dto.InterestRate / 100 / 12;
                decimal totalLoanCost = dto.Amount * (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), dto.TermMonths)) /
                                        ((decimal)Math.Pow((double)(1 + monthlyRate), dto.TermMonths) - 1) * dto.TermMonths;

                bool isHighRisk = clientDebt > averageDebt || (clientDebt + totalLoanCost) > averageDebt;
                if (isHighRisk)
                    return Conflict(new { message = "El cliente es o se convertirá en cliente de alto riesgo." });

                var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
                var random = new Random();
                string loanNumber;
                var allAccounts = await _savingsAccountService.GetAllList();
                do
                {
                    loanNumber = random.Next(100000000, 999999999).ToString();
                } while (loans.Any(l => l.LoanNumber == loanNumber) ||
                         allAccounts.Any(a => a.AccountNumber == loanNumber));

                decimal installmentAmount = dto.Amount * (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), dto.TermMonths)) /
                                            ((decimal)Math.Pow((double)(1 + monthlyRate), dto.TermMonths) - 1);

                var loanDto = new LoanDto
                {
                    LoanNumber = loanNumber,
                    Amount = dto.Amount,
                    InterestRate = dto.InterestRate,
                    TermMonths = dto.TermMonths,
                    TotalInstallments = dto.TermMonths,
                    PaidInstallments = 0,
                    PendingAmount = dto.Amount,
                    IsActive = true,
                    IsLate = false,
                    UserId = dto.ClientId,
                    AdminId = adminId,
                    CreatedAt = DateTime.Now
                };

                await _loanService.CreateLoanAsync(loanDto);

                var savedLoan = (await _loanService.GetAllList()).FirstOrDefault(l => l.LoanNumber == loanNumber);
                if (savedLoan != null)
                {
                    for (int i = 1; i <= dto.TermMonths; i++)
                    {
                        await _loanInstallmentService.AddAsync(new LoanInstallmentDto
                        {
                            LoanId = savedLoan.Id,
                            Amount = installmentAmount,
                            DueDate = DateTime.Now.AddMonths(i),
                            IsPaid = false,
                            IsLate = false,
                            CreatedAt = DateTime.Now
                        });
                    }

                    var primaryAccount = await _savingsAccountService.GetPrimaryAccountByUserIdAsync(dto.ClientId);
                    if (primaryAccount != null)
                        await _savingsAccountService.UpdateBalanceAsync(primaryAccount.AccountNumber, dto.Amount);

                    var user = await _accountService.GetUserByIdAsync(dto.ClientId);
                    if (user != null)
                    {
                        await _emailService.SendAsync(new EmailRequestDto
                        {
                            To = user.Email,
                            Subject = "Préstamo aprobado",
                            HtmlBody = $"Su préstamo ha sido aprobado. Monto: RD$ {dto.Amount:N2}, Plazo: {dto.TermMonths} meses, Tasa: {dto.InterestRate}%, Cuota mensual: RD$ {installmentAmount:N2}."
                        });
                    }
                }

                return StatusCode(StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPatch("{id}/rate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateRate(int id, [FromBody] UpdateRateDto dto)
        {
            try
            {
                var loan = await _loanService.GetById(id);
                if (loan == null)
                    return NotFound(new { message = "Préstamo no encontrado." });

                await _loanService.UpdateInterestRateAsync(id, dto.InterestRate);

                var installments = await _loanInstallmentService.GetAllList();
                var futureInstallments = installments
                    .Where(i => i.LoanId == id && !i.IsPaid && i.DueDate > DateTime.Today)
                    .ToList();

                int remainingMonths = futureInstallments.Count;
                if (remainingMonths > 0)
                {
                    decimal monthlyRate = dto.InterestRate / 100 / 12;
                    decimal newInstallmentAmount = loan.PendingAmount * (monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), remainingMonths)) /
                                                  ((decimal)Math.Pow((double)(1 + monthlyRate), remainingMonths) - 1);

                    foreach (var installment in futureInstallments)
                    {
                        installment.Amount = newInstallmentAmount;
                        await _loanInstallmentService.UpdateAsync(installment.Id, installment);
                    }

                    var user = await _accountService.GetUserByIdAsync(loan.UserId);
                    if (user != null)
                    {
                        await _emailService.SendAsync(new EmailRequestDto
                        {
                            To = user.Email,
                            Subject = "Actualización de tasa de interés",
                            HtmlBody = $"Su tasa de interés ha sido actualizada a {dto.InterestRate}%. Su nueva cuota mensual es RD$ {newInstallmentAmount:N2}."
                        });
                    }
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        private ICreditCardService GetCreditCardService()
        {
            return HttpContext.RequestServices.GetRequiredService<ICreditCardService>();
        }
    }
}