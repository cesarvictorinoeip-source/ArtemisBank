using Asp.Versioning;
using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize(Roles = "Administrador")]
    public class UsersController : BaseApiController
    {
        private readonly IAccountServiceForApi _accountService;
        private readonly ISavingsAccountService _savingsAccountService;

        public UsersController(
            IAccountServiceForApi accountService,
            ISavingsAccountService savingsAccountService)
        {
            _accountService = accountService;
            _savingsAccountService = savingsAccountService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? rol = null)
        {
            try
            {
                var users = await _accountService.GetAllUsersAsync();
                var filtered = users.Where(u => u.UserType != "Comercio").AsQueryable();

                if (!string.IsNullOrEmpty(rol))
                    filtered = filtered.Where(u => u.UserType.ToLower() == rol.ToLower());

                var paged = filtered.OrderByDescending(u => u.UserType)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                if (paged == null || paged.Count == 0)
                    return NoContent();

                return Ok(paged);
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
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var user = await _accountService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new { message = "Usuario no encontrado." });

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("username/{userName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByUserName(string userName)
        {
            try
            {
                var users = await _accountService.GetAllUsersAsync();
                var user = users.FirstOrDefault(u => u.UserName.ToLower() == userName.ToLower());

                if (user == null)
                    return NotFound(new { message = "Usuario no encontrado." });

                return Ok(user);
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
        public async Task<IActionResult> Create([FromBody] SaveUserDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var origin = $"{Request.Scheme}://{Request.Host}";
                var response = await _accountService.RegisterUserAsync(dto, origin);

                if (response.HasError)
                {
                    if (response.Errors.Any(e => e.Contains("ya está en uso")))
                        return Conflict(new { message = response.Errors.FirstOrDefault() });

                    return BadRequest(new { message = response.Errors.FirstOrDefault() });
                }

                if (dto.UserType == "Cliente" && response.UserId != null)
                {
                    var accountNumber = await _savingsAccountService.GenerateUniqueAccountNumberAsync();
                    var savingsAccount = new SavingsAccountDto
                    {
                        AccountNumber = accountNumber,
                        Balance = dto.InitialAmount,
                        IsPrimary = true,
                        IsActive = true,
                        UserId = response.UserId,
                        CreatedAt = DateTime.Now
                    };
                    await _savingsAccountService.AddAsync(savingsAccount);
                }

                return StatusCode(StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(string id, [FromBody] SaveUserDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = await _accountService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new { message = "Usuario no encontrado." });

                dto.Id = id;
                var origin = $"{Request.Scheme}://{Request.Host}";
                var response = await _accountService.EditUserAsync(dto, origin);

                if (response.HasError)
                {
                    if (response.Errors.Any(e => e.Contains("ya está en uso")))
                        return Conflict(new { message = response.Errors.FirstOrDefault() });

                    return BadRequest(new { message = response.Errors.FirstOrDefault() });
                }

                if (dto.UserType == "Cliente" && dto.AdditionalAmount > 0)
                {
                    var primaryAccount = await _savingsAccountService.GetPrimaryAccountByUserIdAsync(id);
                    if (primaryAccount != null)
                        await _savingsAccountService.UpdateBalanceAsync(primaryAccount.AccountNumber, dto.AdditionalAmount);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPatch("{id}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangeStatus(string id, [FromBody] ChangeStatusDto dto)
        {
            try
            {
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (currentUserId == id)
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "No puede modificar su propio estado." });

                var user = await _accountService.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound(new { message = "Usuario no encontrado." });

                var response = await _accountService.ChangeUserStatusAsync(id, dto.Status);

                if (response.HasError)
                    return BadRequest(new { message = response.Errors.FirstOrDefault() });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}