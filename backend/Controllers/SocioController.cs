namespace Backend.Controllers;

using Backend.DTOs.Socio;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SocioController : ControllerBase
{
    private readonly ISocioService _socioService;

    public SocioController(ISocioService socioService)
    {
        _socioService = socioService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? estado = null,
        [FromQuery] int? idPlan = null,
        [FromQuery] int? idActividad = null)
    {
        var result = await _socioService.GetPagedAsync(page, pageSize, search, estado, idPlan, idActividad);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("coaches")]
    public async Task<IActionResult> GetCoaches()
    {
        var coaches = await _socioService.GetCoachesAsync();
        return Ok(coaches);
    }

    [HttpGet("mi-socio")]
    public async Task<IActionResult> GetMiSocio()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var socio = await _socioService.GetByUsuarioIdAsync(userId);
        if (socio == null)
        {
            return NotFound(new { message = "Socio no encontrado para el usuario actual." });
        }
        return Ok(socio);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var socio = await _socioService.GetByIdAsync(id);
        if (socio == null)
        {
            return NotFound(new { errors = new[] { $"El socio con ID {id} no existe." } });
        }
        return Ok(socio);
    }

    [HttpGet("buscar")]
    public async Task<IActionResult> Buscar([FromQuery] string q = "", [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 3)
        {
            return BadRequest(new { errors = new[] { "La consulta de búsqueda debe tener al menos 3 caracteres." } });
        }
        var result = await _socioService.BuscarAsync(q, limit);
        return Ok(result);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SocioCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var modelErrors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new { errors = modelErrors });
        }

        var (success, data, errors) = await _socioService.CreateAsync(dto);
        if (!success)
        {
            return BadRequest(new { errors });
        }

        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, data);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SocioCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var modelErrors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new { errors = modelErrors });
        }

        var (success, data, errors, notFound) = await _socioService.UpdateAsync(id, dto);
        if (notFound)
        {
            return NotFound(new { errors = new[] { $"El socio con ID {id} no existe." } });
        }
        if (!success)
        {
            return BadRequest(new { errors });
        }

        return Ok(data);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> ChangeEstado(int id, [FromBody] SocioEstadoUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var modelErrors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new { errors = modelErrors });
        }

        var (success, data, errors, notFound) = await _socioService.ChangeEstadoAsync(id, dto.Estado);
        if (notFound)
        {
            return NotFound(new { errors = new[] { $"El socio con ID {id} no existe." } });
        }
        if (!success)
        {
            return BadRequest(new { errors });
        }

        return Ok(data);
    }
}
