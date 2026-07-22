namespace Backend.Controllers;

using Backend.DTOs.Plan;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
