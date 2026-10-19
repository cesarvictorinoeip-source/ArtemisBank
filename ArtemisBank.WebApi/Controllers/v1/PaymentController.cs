using Asp.Versioning;
using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.Interfaces.Services;
using ArtemisBank.Core.Application.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize(Roles = "Administrador,Comercio")]
    [Route("pay")]
    public class PaymentController : BaseApiController
    {
        private readonly ICommerceService _commerceService;
        private readonly ICreditCardService _creditCardService;
        private readonly ICreditCardConsumptionService _consumptionService;
        private readonly ISavingsAccountService _savingsAccountService;
        private readonly ITransactionService _transactionService;
        private readonly IAccountServiceForApi _accountService;
        private readonly IEmailService _emailService;

        public PaymentController(
            ICommerceService commerceService,
            ICreditCardService creditCardService,
            ICreditCardConsumptionService consumptionService,
            ISavingsAccountService savingsAccountService,
            ITransactionService transactionService,
            IAccountServiceForApi accountService,
            IEmailService emailService)
        {
            _commerceService = commerceService;
            _creditCardService = creditCardService;
            _consumptionService = consumptionService;
            _savingsAccountService = savingsAccountService;
            _transactionService = transactionService;
            _accountService = accountService;
            _emailService = emailService;
        }

        [HttpGet("get-transactions/{commerceId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTransactions(int commerceId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                // Si es comercio, usa su propio ID del token
                if (User.IsInRole("Comercio"))
                {
                    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    var commerces = await _commerceService.GetAllList();
                    var userCommerce = commerces.FirstOrDefault(c => c.UserId == userId);
                    if (userCommerce != null)
                        commerceId = userCommerce.Id;
                }

                var commerce = await _commerceService.GetById(commerceId);
                if (commerce == null)
                    return NotFound(new { message = "Comercio no encontrado." });

                var primaryAccount = await _savingsAccountService.GetPrimaryAccountByUserIdAsync(commerce.UserId!);
                if (primaryAccount == null)
                    return NoContent();

                var transactions = await _transactionService.GetAllList();
                var commerceTransactions = transactions
                    .Where(t => t.SavingsAccountId == primaryAccount.Id)
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                if (commerceTransactions.Count == 0)
                    return NoContent();

                return Ok(commerceTransactions);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("process-payment/{commerceId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProcessPayment(int commerceId, [FromBody] ProcessPaymentDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Si es comercio, usa su propio ID del token
                if (User.IsInRole("Comercio"))
                {
                    var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    var commerces = await _commerceService.GetAllList();
                    var userCommerce = commerces.FirstOrDefault(c => c.UserId == userId);
                    if (userCommerce != null)
                        commerceId = userCommerce.Id;
                }

                var commerce = await _commerceService.GetById(commerceId);
                if (commerce == null)
                    return BadRequest(new { message = "Comercio no encontrado." });

                // Validar tarjeta
                var cards = await _creditCardService.GetAllList();
                var card = cards.FirstOrDefault(c => c.CardNumber == dto.CardNumber && c.IsActive);
                if (card == null)
                    return BadRequest(new { message = "Tarjeta no válida o inactiva." });

                // Validar fecha expiración
                var expParts = card.ExpirationDate.Split('/');
                var expDate = new DateTime(2000 + int.Parse(expParts[1]), int.Parse(expParts[0]), 1).AddMonths(1).AddDays(-1);
                if (expDate < DateTime.Now)
                    return BadRequest(new { message = "La tarjeta está vencida." });

                // Validar CVC
                string hashedCvc = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(dto.CVC)));
                if (card.CVC != hashedCvc)
                    return BadRequest(new { message = "CVC inválido." });

                // Validar límite
                decimal availableCredit = card.CreditLimit - card.AmountOwed;
                if (dto.TransactionAmount > availableCredit)
                    return BadRequest(new { message = "El monto excede el crédito disponible." });

                // Procesar pago
                card.AmountOwed += dto.TransactionAmount;
                await _creditCardService.UpdateAsync(card.Id, card);

                // Acreditar a cuenta del comercio
                var commercePrimaryAccount = await _savingsAccountService.GetPrimaryAccountByUserIdAsync(commerce.UserId!);
                if (commercePrimaryAccount != null)
                    await _savingsAccountService.UpdateBalanceAsync(commercePrimaryAccount.AccountNumber, dto.TransactionAmount);

                // Registrar consumo
                await _consumptionService.AddAsync(new CreditCardConsumptionDto
                {
                    CreditCardId = card.Id,
                    Amount = dto.TransactionAmount,
                    Commerce = commerce.Name,
                    Status = "Aprobado",
                    CreatedAt = DateTime.Now
                });

                // Registrar transacción en cuenta del comercio
                if (commercePrimaryAccount != null)
                {
                    await _transactionService.AddAsync(new TransactionDto
                    {
                        SavingsAccountId = commercePrimaryAccount.Id,
                        Amount = dto.TransactionAmount,
                        Type = "Credito",
                        Beneficiary = commercePrimaryAccount.AccountNumber,
                        Origin = card.CardNumber.Substring(card.CardNumber.Length - 4),
                        Status = "Aprobada",
                        CreatedAt = DateTime.Now
                    });
                }

                // Correo al cliente
                var cardOwner = await _accountService.GetUserByIdAsync(card.UserId);
                if (cardOwner != null)
                {
                    string lastFour = card.CardNumber.Substring(card.CardNumber.Length - 4);
                    await _emailService.SendAsync(new EmailRequestDto
                    {
                        To = cardOwner.Email,
                        Subject = $"Consumo realizado con la tarjeta {lastFour}",
                        HtmlBody = $"Se ha realizado un consumo de RD$ {dto.TransactionAmount:N2} con su tarjeta terminada en {lastFour} en {commerce.Name}. Fecha y hora: {DateTime.Now:dd/MM/yyyy HH:mm:ss}."
                    });
                }

                // Correo al comercio
                var commerceUser = await _accountService.GetUserByIdAsync(commerce.UserId!);
                if (commerceUser != null)
                {
                    string lastFour = card.CardNumber.Substring(card.CardNumber.Length - 4);
                    await _emailService.SendAsync(new EmailRequestDto
                    {
                        To = commerceUser.Email,
                        Subject = $"Pago recibido a través de tarjeta {lastFour}",
                        HtmlBody = $"Ha recibido un pago de RD$ {dto.TransactionAmount:N2} a través de la tarjeta terminada en {lastFour} en {commerce.Name}. Fecha y hora: {DateTime.Now:dd/MM/yyyy HH:mm:ss}."
                    });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}