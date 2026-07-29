namespace Backend.Controllers;

using Backend.DTOs.Usuario;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Administrador")]
[ApiController]
[Route("api/[controller]")]
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuarioController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet("coaches")]
    public async Task<IActionResult> GetCoaches([FromQuery] string? search = null)
    {
        var coaches = await _usuarioService.GetCoachesListAsync(search);
        return Ok(coaches);
    }

    [HttpPost("coach")]
    public async Task<IActionResult> CreateCoach([FromBody] CreateCoachDto dto)
    {
        if (!ModelState.IsValid)
        {
            var modelErrors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new { errors = modelErrors });
        }

        var (success, data, errors) = await _usuarioService.CreateCoachAsync(dto);
        if (!success)
        {
            return BadRequest(new { errors });
        }

        return Ok(data);
    }

    [HttpPut("coach/{id:int}")]
    public async Task<IActionResult> UpdateCoach(int id, [FromBody] UpdateCoachDto dto)
    {
        if (!ModelState.IsValid)
        {
            var modelErrors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new { errors = modelErrors });
        }

        var (success, data, errors, notFound) = await _usuarioService.UpdateCoachAsync(id, dto);
        if (notFound)
        {
            return NotFound(new { errors = new[] { $"El coach con ID {id} no existe." } });
        }
        if (!success)
        {
            return BadRequest(new { errors });
        }

        return Ok(data);
    }

    [HttpDelete("coach/{id:int}")]
    public async Task<IActionResult> DeleteCoach(int id)
    {
        var (success, errors, notFound) = await _usuarioService.DeleteCoachAsync(id);
        if (notFound)
        {
            return NotFound(new { errors = new[] { $"El coach con ID {id} no existe." } });
        }
        if (!success)
        {
            return BadRequest(new { errors });
        }

        return Ok(new { message = "Coach deshabilitado correctamente." });
    }
}
