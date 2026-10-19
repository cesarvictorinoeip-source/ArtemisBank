using Asp.Versioning;
using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace ArtemisBank.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize(Roles = "Administrador")]
    public class CreditCardController : BaseApiController
    {
        private readonly ICreditCardService _creditCardService;
        private readonly ICreditCardConsumptionService _consumptionService;
        private readonly IAccountServiceForApi _accountService;
        private readonly IEmailService _emailService;

        public CreditCardController(
            ICreditCardService creditCardService,
            ICreditCardConsumptionService consumptionService,
            IAccountServiceForApi accountService,
            IEmailService emailService)
        {
            _creditCardService = creditCardService;
            _consumptionService = consumptionService;
            _accountService = accountService;
            _emailService = emailService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] string? cedula = null, [FromQuery] string? estado = null)
        {
            try
            {
                var cards = await _creditCardService.GetAllList();
                var users = await _accountService.GetAllUsersAsync();
                var filtered = cards.AsQueryable();

                if (!string.IsNullOrEmpty(cedula))
                {
                    var user = users.FirstOrDefault(u => u.IdCard == cedula);
                    if (user != null)
                        filtered = filtered.Where(c => c.UserId == user.Id);
                }

                if (estado == "activa")
                    filtered = filtered.Where(c => c.IsActive);
                else if (estado == "cancelada")
                    filtered = filtered.Where(c => !c.IsActive);

                var pageSize = 20;
                var result = filtered.OrderByDescending(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new
                    {
                        c.Id,
                        c.CardNumber,
                        c.CreditLimit,
                        c.AmountOwed,
                        c.ExpirationDate,
                        c.IsActive,
                        c.UserId,
                        OwnerFullName = (users.FirstOrDefault(u => u.Id == c.UserId) != null
                            ? users.FirstOrDefault(u => u.Id == c.UserId)!.FirstName + " " +
                              users.FirstOrDefault(u => u.Id == c.UserId)!.LastName
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
                var card = await _creditCardService.GetById(id);
                if (card == null)
                    return NotFound(new { message = "Tarjeta no encontrada." });

                var consumptions = await _consumptionService.GetAllList();
                card.Consumptions = consumptions
                    .Where(c => c.CreditCardId == id)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToList();

                return Ok(card);
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
        public async Task<IActionResult> Create([FromBody] CreditCardDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
                var random = new Random();
                string cardNumber;
                var allCards = await _creditCardService.GetAllList();

                do
                {
                    cardNumber = string.Concat(Enumerable.Range(0, 4).Select(_ => random.Next(1000, 9999).ToString()));
                } while (allCards.Any(c => c.CardNumber == cardNumber));

                string cvc = random.Next(100, 999).ToString();
                string hashedCvc = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(cvc)));
                string expirationDate = DateTime.Now.AddYears(3).ToString("MM/yy");

                var cardDto = new CreditCardDto
                {
                    CardNumber = cardNumber,
                    CreditLimit = dto.CreditLimit,
                    AmountOwed = 0,
                    ExpirationDate = expirationDate,
                    CVC = hashedCvc,
                    IsActive = true,
                    UserId = dto.UserId,
                    AdminId = adminId,
                    CreatedAt = DateTime.Now
                };

                await _creditCardService.AddAsync(cardDto);
                return StatusCode(StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPatch("{id}/limit")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateLimit(int id, [FromBody] CreditCardDto dto)
        {
            try
            {
                var card = await _creditCardService.GetById(id);
                if (card == null)
                    return NotFound(new { message = "Tarjeta no encontrada." });

                if (dto.CreditLimit < card.AmountOwed)
                    return BadRequest(new { message = "El nuevo límite no puede ser inferior a la deuda actual." });

                await _creditCardService.UpdateLimitAsync(id, dto.CreditLimit);

                var user = await _accountService.GetUserByIdAsync(card.UserId);
                if (user != null)
                {
                    string lastFour = card.CardNumber.Substring(card.CardNumber.Length - 4);
                    await _emailService.SendAsync(new EmailRequestDto
                    {
                        To = user.Email,
                        Subject = $"Actualización de límite de tarjeta {lastFour}",
                        HtmlBody = $"El límite de su tarjeta terminada en {lastFour} ha sido modificado. Nuevo límite: RD$ {dto.CreditLimit:N2}."
                    });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPatch("{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var card = await _creditCardService.GetById(id);
                if (card == null)
                    return NotFound(new { message = "Tarjeta no encontrada." });

                if (card.AmountOwed > 0)
                    return BadRequest(new { message = "Para cancelar esta tarjeta, el cliente debe saldar la totalidad de la deuda pendiente." });

                await _creditCardService.CancelAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}