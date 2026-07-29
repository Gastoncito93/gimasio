using Backend.DTOs.Progreso;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProgresoController : ControllerBase
{
    private readonly ISocioProgresoService _progresoService;
    private readonly IWebHostEnvironment _env;

    public ProgresoController(ISocioProgresoService progresoService, IWebHostEnvironment env)
    {
        _progresoService = progresoService;
        _env = env;
    }

    [Authorize]
    [HttpGet("socio/{idSocio}")]
    public async Task<IActionResult> GetBySocio(int idSocio)
    {
        var result = await _progresoService.GetProgresosBySocioAsync(idSocio);
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Crear([FromForm] CrearSocioProgresoDto dto)
    {
        if (!ModelState.IsValid)
        {
            var modelErrors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new { errors = modelErrors });
        }

        var webRootPath = _env.WebRootPath;
        if (string.IsNullOrEmpty(webRootPath))
        {
            webRootPath = Path.Combine(_env.ContentRootPath, "wwwroot");
        }

        var (success, data, error) = await _progresoService.CrearProgresoAsync(dto, webRootPath);
        if (!success || data == null)
        {
            return BadRequest(new { errors = new[] { error ?? "Error al guardar el registro de progreso." } });
        }

        return Ok(data);
    }

    [Authorize(Roles = "Administrador,Coach")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var webRootPath = _env.WebRootPath;
        if (string.IsNullOrEmpty(webRootPath))
        {
            webRootPath = Path.Combine(_env.ContentRootPath, "wwwroot");
        }

        var (success, error) = await _progresoService.EliminarProgresoAsync(id, webRootPath);
        if (!success)
        {
            return BadRequest(new { errors = new[] { error ?? "Error al eliminar el registro." } });
        }

        return Ok(new { message = "Registro de progreso eliminado correctamente." });
    }
}
