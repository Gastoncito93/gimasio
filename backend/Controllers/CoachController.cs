using System.Security.Claims;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Authorize(Roles = "Coach,Administrador")]
[ApiController]
[Route("api/[controller]")]
public class CoachController : ControllerBase
{
    private readonly ICoachService _coachService;

    public CoachController(ICoachService coachService)
    {
        _coachService = coachService;
    }

    [HttpGet("alumnos")]
    public async Task<IActionResult> GetMisAlumnos([FromQuery] string? search = null)
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

        if (idClaim == null || !int.TryParse(idClaim, out int userId) || roleClaim == null)
        {
            return Unauthorized(new { errors = new[] { "Usuario no identificado o token no válido." } });
        }

        var alumnos = await _coachService.GetMisAlumnosAsync(userId, roleClaim, search);
        return Ok(alumnos);
    }

    [HttpGet("alumnos/{id:int}")]
    public async Task<IActionResult> GetAlumnoDetalle(int id)
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

        if (idClaim == null || !int.TryParse(idClaim, out int userId) || roleClaim == null)
        {
            return Unauthorized(new { errors = new[] { "Usuario no identificado o token no válido." } });
        }

        var (success, data, error, isForbidden) = await _coachService.GetAlumnoDetalleAsync(id, userId, roleClaim);

        if (isForbidden)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { errors = new[] { error ?? "Acceso denegado." } });
        }

        if (!success || data == null)
        {
            return NotFound(new { errors = new[] { error ?? $"El alumno con ID {id} no existe." } });
        }

        return Ok(data);
    }
}
