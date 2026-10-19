using Asp.Versioning;
using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize(Roles = "Administrador")]
    public class SavingsAccountController : BaseApiController
    {
        private readonly ISavingsAccountService _savingsAccountService;
        private readonly IAccountServiceForApi _accountService;
        private readonly ITransactionService _transactionService;

        public SavingsAccountController(
            ISavingsAccountService savingsAccountService,
            IAccountServiceForApi accountService,
            ITransactionService transactionService)
        {
            _savingsAccountService = savingsAccountService;
            _accountService = accountService;
            _transactionService = transactionService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] string? cedula = null, [FromQuery] string? estado = null, [FromQuery] string? tipo = null)
        {
            try
            {
                var accounts = await _savingsAccountService.GetAllList();
                var users = await _accountService.GetAllUsersAsync();
                var filtered = accounts.AsQueryable();

                if (!string.IsNullOrEmpty(cedula))
                {
                    var user = users.FirstOrDefault(u => u.IdCard == cedula);
                    if (user != null)
                        filtered = filtered.Where(a => a.UserId == user.Id);
                }

                if (estado == "activo")
                    filtered = filtered.Where(a => a.IsActive);
                else if (estado == "cancelado")
                    filtered = filtered.Where(a => !a.IsActive);

                if (tipo == "principal")
                    filtered = filtered.Where(a => a.IsPrimary);
                else if (tipo == "secundaria")
                    filtered = filtered.Where(a => !a.IsPrimary);

                var pageSize = 20;
                var result = filtered.OrderByDescending(a => a.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new
                    {
                        a.Id,
                        a.AccountNumber,
                        a.Balance,
                        a.IsPrimary,
                        a.IsActive,
                        a.UserId,
                        OwnerFullName = (users.FirstOrDefault(u => u.Id == a.UserId) != null
                            ? users.FirstOrDefault(u => u.Id == a.UserId)!.FirstName + " " +
                              users.FirstOrDefault(u => u.Id == a.UserId)!.LastName
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

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] SavingsAccountDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var accountNumber = await _savingsAccountService.GenerateUniqueAccountNumberAsync();

                var accountDto = new SavingsAccountDto
                {
                    AccountNumber = accountNumber,
                    Balance = dto.Balance,
                    IsPrimary = false,
                    IsActive = true,
                    UserId = dto.UserId,
                    CreatedAt = DateTime.Now
                };

                await _savingsAccountService.AddAsync(accountDto);
                return StatusCode(StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("{accountNumber}/transactions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTransactions(string accountNumber)
        {
            try
            {
                var account = await _savingsAccountService.GetByAccountNumberAsync(accountNumber);
                if (account == null)
                    return NotFound(new { message = "Cuenta no encontrada." });

                var transactions = await _transactionService.GetAllList();
                var accountTransactions = transactions
                    .Where(t => t.SavingsAccountId == account.Id)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToList();

                return Ok(accountTransactions);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}