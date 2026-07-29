namespace Backend.Controllers;

using Backend.DTOs.Cuota;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CuotaController : ControllerBase
{
    private readonly ICuotaService _cuotaService;

    public CuotaController(ICuotaService cuotaService)
    {
        _cuotaService = cuotaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? estado = null,
        [FromQuery] int? periodo = null)
    {
        var result = await _cuotaService.GetPagedAsync(page, pageSize, search, estado, periodo);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _cuotaService.GetByIdAsync(id);
        if (result == null)
        {
            return NotFound(new { errors = new[] { $"La cuota con ID {id} no existe." } });
        }
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CuotaCreateDto dto)
    {
        var (success, data, errors) = await _cuotaService.CreateAsync(dto);
        if (!success)
        {
            return BadRequest(new { errors });
        }
        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, data);
    }

    [HttpPut("{id:int}/observacion")]
    public async Task<IActionResult> UpdateObservacion(int id, [FromBody] CuotaObservacionUpdateDto dto)
    {
        var (success, data, errors, notFound) = await _cuotaService.UpdateObservacionAsync(id, dto);
        if (notFound)
        {
            return NotFound(new { errors = new[] { $"La cuota con ID {id} no existe." } });
        }
        if (!success)
        {
            return BadRequest(new { errors });
        }
        return Ok(data);
    }

    [HttpPatch("{id:int}/pagar")]
    public async Task<IActionResult> Pagar(int id, [FromBody] CuotaPagoDto dto)
    {
        var (success, data, errors, notFound) = await _cuotaService.PagarAsync(id, dto);
        if (notFound)
        {
            return NotFound(new { errors = new[] { $"La cuota con ID {id} no existe." } });
        }
        if (!success)
        {
            return BadRequest(new { errors });
        }
        return Ok(data);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPatch("{id:int}/anular")]
    public async Task<IActionResult> Anular(int id)
    {
        var (success, data, errors, notFound) = await _cuotaService.AnularAsync(id);
        if (notFound)
        {
            return NotFound(new { errors = new[] { $"La cuota con ID {id} no existe." } });
        }
        if (!success)
        {
            return BadRequest(new { errors });
        }
        return Ok(data);
    }
}
