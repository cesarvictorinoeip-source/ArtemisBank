using Asp.Versioning;
using ArtemisBank.Core.Application.DTOs;
using ArtemisBank.Core.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtemisBank.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Authorize(Roles = "Administrador")]
    public class CommerceController : BaseApiController
    {
        private readonly ICommerceService _commerceService;

        public CommerceController(ICommerceService commerceService)
        {
            _commerceService = commerceService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get([FromQuery] int? page = null, [FromQuery] int? pageSize = null)
        {
            try
            {
                var commerces = await _commerceService.GetAllList();
                var active = commerces.Where(c => c.IsActive).OrderByDescending(c => c.CreatedAt).ToList();

                if (page.HasValue && pageSize.HasValue)
                    active = active.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();

                if (active.Count == 0)
                    return NoContent();

                return Ok(active);
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
                var commerce = await _commerceService.GetById(id);
                if (commerce == null)
                    return NotFound(new { message = "Comercio no encontrado." });

                return Ok(commerce);
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
        public async Task<IActionResult> Create([FromBody] CommerceDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                dto.IsActive = true;
                dto.CreatedAt = DateTime.Now;
                await _commerceService.AddAsync(dto);
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
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] CommerceDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var commerce = await _commerceService.GetById(id);
                if (commerce == null)
                    return NotFound(new { message = "Comercio no encontrado." });

                dto.Id = id;
                dto.UpdatedAt = DateTime.Now;
                await _commerceService.UpdateAsync(id, dto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStatusDto dto)
        {
            try
            {
                var commerce = await _commerceService.GetById(id);
                if (commerce == null)
                    return NotFound(new { message = "Comercio no encontrado." });

                commerce.IsActive = dto.Status;
                commerce.UpdatedAt = DateTime.Now;
                await _commerceService.UpdateAsync(id, commerce);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}