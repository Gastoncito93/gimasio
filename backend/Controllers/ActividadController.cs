using Backend.DTOs.Actividad;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ActividadController : ControllerBase
{
    private readonly IActividadService _actividadService;

    public ActividadController(IActividadService actividadService)
    {
        _actividadService = actividadService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search = null, [FromQuery] string? estado = null)
    {
        var actividades = await _actividadService.GetAllAsync(search, estado);
        return Ok(actividades);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var actividad = await _actividadService.GetByIdAsync(id);
        if (actividad == null)
        {
            return NotFound(new { errors = new[] { $"La actividad con ID {id} no existe." } });
        }
        return Ok(actividad);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ActividadCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var modelErrors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new { errors = modelErrors });
        }

        var (success, data, errors) = await _actividadService.CreateAsync(dto);
        if (!success)
        {
            return BadRequest(new { errors });
        }

        return CreatedAtAction(nameof(GetById), new { id = data!.Id }, data);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ActividadCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var modelErrors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new { errors = modelErrors });
        }

        var (success, data, errors, notFound) = await _actividadService.UpdateAsync(id, dto);
        if (notFound)
        {
            return NotFound(new { errors = new[] { $"La actividad con ID {id} no existe." } });
        }
        if (!success)
        {
            return BadRequest(new { errors });
        }

        return Ok(data);
    }

    [Authorize(Roles = "Administrador")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, errors, notFound) = await _actividadService.DeleteAsync(id);
        if (notFound)
        {
            return NotFound(new { errors = new[] { $"La actividad con ID {id} no existe." } });
        }
        if (!success)
        {
            return BadRequest(new { errors });
        }

        return Ok(new { message = "Actividad eliminada o inactivada correctamente." });
    }
}
