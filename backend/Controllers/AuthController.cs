using System.Security.Claims;
using Backend.DTOs.Auth;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IWebHostEnvironment _env;

    public AuthController(IAuthService authService, IWebHostEnvironment env)
    {
        _authService = authService;
        _env = env;
    }

    [EnableRateLimiting("LoginPolicy")]
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

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        if (!ModelState.IsValid)
        {
            var modelErrors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new { errors = modelErrors });
        }

        var (success, data, error) = await _authService.RegisterAsync(dto);
        if (!success)
        {
            return BadRequest(new { errors = new[] { error ?? "Error al registrar usuario." } });
        }

        return Ok(data);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (idClaim == null || !int.TryParse(idClaim, out int userId))
        {
            return Unauthorized(new { errors = new[] { "Usuario no identificado o sesión no válida." } });
        }

        var (success, userDto, error) = await _authService.GetUserProfileAsync(userId);
        if (!success || userDto == null)
        {
            return Unauthorized(new { errors = new[] { error ?? "Usuario no encontrado." } });
        }

        return Ok(userDto);
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        if (!ModelState.IsValid)
        {
            var modelErrors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new { errors = modelErrors });
        }

        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (idClaim == null || !int.TryParse(idClaim, out int userId))
        {
            return Unauthorized(new { errors = new[] { "Usuario no identificado o sesión no válida." } });
        }

        var (success, userDto, error) = await _authService.UpdateProfileAsync(userId, dto);
        if (!success || userDto == null)
        {
            return BadRequest(new { errors = new[] { error ?? "Error al actualizar perfil." } });
        }

        return Ok(userDto);
    }

    [Authorize]
    [HttpPost("avatar")]
    public async Task<IActionResult> UploadAvatar([FromForm] IFormFile avatar)
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (idClaim == null || !int.TryParse(idClaim, out int userId))
        {
            return Unauthorized(new { errors = new[] { "Usuario no identificado o sesión no válida." } });
        }

        var webRootPath = _env.WebRootPath;
        if (string.IsNullOrEmpty(webRootPath))
        {
            webRootPath = Path.Combine(_env.ContentRootPath, "wwwroot");
        }

        var (success, rutaAvatar, error) = await _authService.UploadAvatarAsync(userId, avatar, webRootPath);
        if (!success)
        {
            return BadRequest(new { errors = new[] { error ?? "Error al subir avatar." } });
        }

        return Ok(new { rutaAvatar });
    }
}
