using System.Security.Claims;
using Backend.DTOs.Auth;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            var modelErrors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new { errors = modelErrors });
        }

        var (success, data, error) = await _authService.LoginAsync(dto);
        if (!success)
        {
            return Unauthorized(new { errors = new[] { error ?? "Credenciales incorrectas." } });
        }

        return Ok(data);
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var usernameClaim = User.FindFirst(ClaimTypes.Name)?.Value;
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

        if (idClaim == null || usernameClaim == null || roleClaim == null)
        {
            return Unauthorized(new { errors = new[] { "Usuario no identificado o sesión no válida." } });
        }

        return Ok(new UserBasicInfoDto
        {
            Id = int.Parse(idClaim),
            Username = usernameClaim,
            Nombre = usernameClaim,
            Rol = roleClaim
        });
    }

    [Authorize(Roles = "Administrador")]
    [HttpGet("solo-admin")]
    public IActionResult SoloAdmin()
    {
        return Ok(new { message = "Acceso exitoso. Eres Administrador." });
    }
}
