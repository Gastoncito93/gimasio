namespace Backend.Controllers;

using Backend.DTOs.Plan;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PlanController : ControllerBase
{
    private readonly IPlanService _planService;

    public PlanController(IPlanService planService)
    {
        _planService = planService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        var result = await _planService.GetPagedAsync(page, pageSize, search);
        return Ok(result);
    }

    [HttpGet("buscar")]
    public async Task<IActionResult> Buscar([FromQuery] string q = "", [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
        {
            return BadRequest(new { errors = new[] { "La consulta de búsqueda debe tener al menos 2 caracteres." } });
        }
        var result = await _planService.BuscarAsync(q, limit);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var plan = await _planService.GetByIdAsync(id);
        if (plan == null)
        {
            return NotFound(new { errors = new[] { $"El plan con ID {id} no existe." } });
        }
        return Ok(plan);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PlanCreateUpdateDto dto)
    {
        var (success, data, errors) = await _planService.CreateAsync(dto);
        if (!success)
        {
            return BadRequest(new { errors });
        }
        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, data);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] PlanCreateUpdateDto dto)
    {
        var (success, data, errors, notFound) = await _planService.UpdateAsync(id, dto);
        if (notFound)
        {
            return NotFound(new { errors = new[] { $"El plan con ID {id} no existe." } });
        }
        if (!success)
        {
            return BadRequest(new { errors });
        }
        return Ok(data);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> ChangeEstado(int id, [FromBody] PlanEstadoUpdateDto dto)
    {
        var (success, data, errors, notFound) = await _planService.ChangeEstadoAsync(id, dto);
        if (notFound)
        {
            return NotFound(new { errors = new[] { $"El plan con ID {id} no existe." } });
        }
        if (!success)
        {
            return BadRequest(new { errors });
        }
        return Ok(data);
    }
}
