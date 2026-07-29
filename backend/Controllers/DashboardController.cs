namespace Backend.Controllers;

using Backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Administrador")]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var totalAlumnos = await _context.Socios.CountAsync();
        var totalCoaches = await _context.Usuarios.CountAsync(u => u.IdRol == 2 && u.EliminadoAt == null);
        var totalPlanesVigentes = await _context.Planes.CountAsync(p => p.Estado == "Activo");

        var now = DateTime.UtcNow;
        var inicioMes = new DateTime(now.Year, now.Month, 1);
        var recaudadoEsteMes = await _context.Cuotas
            .Where(c => c.Estado == "Pagada" && c.FechaPago.HasValue && c.FechaPago.Value >= inicioMes)
            .SumAsync(c => (decimal?)c.Monto) ?? 0m;

        return Ok(new
        {
            totalAlumnos,
            totalCoaches,
            totalPlanesVigentes,
            recaudadoEsteMes
        });
    }
}
