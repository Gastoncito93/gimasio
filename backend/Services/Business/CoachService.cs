namespace Backend.Services.Business;

using Backend.Data;
using Backend.DTOs.Coach;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class CoachService : ICoachService
{
    private readonly AppDbContext _context;

    public CoachService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CoachAlumnoDto>> GetMisAlumnosAsync(int userId, string userRole, string? search = null)
    {
        IQueryable<Socio> query = _context.Socios
            .Include(s => s.Plan)
            .Include(s => s.Cuotas)
            .Include(s => s.Coach).ThenInclude(c => c!.Actividad)
            .Include(s => s.Usuario)
            .Include(s => s.Progresos)
            .AsSplitQuery()
            .AsNoTracking();

        // Si es Coach, filtrar estrictamente por los alumnos asignados a su ID
        if (userRole == "Coach")
        {
            query = query.Where(s => s.IdCoach == userId);
        }

        // Filtro de búsqueda por nombre, DNI o usuario
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(s => s.NombreCompleto.ToLower().Contains(searchLower) ||
                                     s.Dni.ToLower().Contains(searchLower) ||
                                     (s.Usuario != null && s.Usuario.Username.ToLower().Contains(searchLower)));
        }

        var alumnos = await query.OrderBy(s => s.NombreCompleto).ToListAsync();

        return alumnos.Select(s =>
        {
            var cuotasPendientes = s.Cuotas.Count(c => c.Estado == "Pendiente" || c.Estado == "Vencida");
            var deudaEstado = cuotasPendientes == 0 ? "Al día" : $"{cuotasPendientes} cuota(s) pendiente(s)";

            return new CoachAlumnoDto
            {
                Id = s.Id,
                Dni = s.Dni,
                Nombre = s.NombreCompleto,
                NombreCompleto = s.NombreCompleto,
                Username = s.Usuario?.Username,
                Avatar = s.Usuario?.RutaAvatar,
                Estado = s.Estado,
                PlanNombre = s.Plan?.Nombre ?? "Sin plan",
                DeudaEstado = deudaEstado,
                Progreso = "No disponible todavía",
                UltimaSesion = "No disponible todavía",
                Observaciones = s.Observacion,
                CoachNombre = s.Coach?.Nombre ?? "Sin asignación",
                ActividadNombre = s.Coach?.Actividad?.Nombre ?? "Musculación",
                CantidadEvoluciones = s.Progresos.Count
            };
        }).ToList();
    }

    public async Task<(bool Success, CoachAlumnoDetalleDto? Data, string? Error, bool IsForbidden)> GetAlumnoDetalleAsync(int alumnoId, int requestingUserId, string requestingUserRole)
    {
        var socio = await _context.Socios
            .Include(s => s.Plan)
            .Include(s => s.Cuotas)
            .Include(s => s.Coach).ThenInclude(c => c!.Actividad)
            .Include(s => s.Usuario)
            .Include(s => s.Progresos)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == alumnoId);

        if (socio == null)
        {
            return (false, null, $"El alumno con ID {alumnoId} no existe.", false);
        }

        // Validación de seguridad estricta en el backend:
        // Si el usuario es Coach, debe ser el Coach asignado al alumno (IdCoach == requestingUserId)
        if (requestingUserRole == "Coach" && socio.IdCoach != requestingUserId)
        {
            return (false, null, "No tienes permisos para acceder a este alumno.", true);
        }

        var cuotasPendientes = socio.Cuotas.Where(c => c.Estado == "Pendiente" || c.Estado == "Vencida").ToList();
        var deudaEstado = cuotasPendientes.Count == 0 ? "Al día" : $"{cuotasPendientes.Count} cuota(s) pendiente(s)";
        var proximaCuota = cuotasPendientes.OrderBy(c => c.FechaVencimiento).FirstOrDefault();

        var dto = new CoachAlumnoDetalleDto
        {
            Id = socio.Id,
            Nombre = socio.NombreCompleto,
            Dni = socio.Dni,
            Username = socio.Usuario?.Username,
            Avatar = socio.Usuario?.RutaAvatar,
            Telefono = socio.Telefono,
            Email = socio.Email,
            Estado = socio.Estado,
            FechaAlta = socio.FechaAlta,
            Observaciones = socio.Observacion,

            IdPlan = socio.IdPlan,
            PlanNombre = socio.Plan?.Nombre ?? "Sin plan",
            PlanPrecio = socio.Plan?.PrecioMensual ?? 0,

            IdCoach = socio.IdCoach,
            CoachNombre = socio.Coach?.Nombre ?? "Sin asignación",
            ActividadNombre = socio.Coach?.Actividad?.Nombre ?? "Musculación",

            DeudaEstado = deudaEstado,
            CuotasPendientesCount = cuotasPendientes.Count,
            ProximoVencimiento = proximaCuota != null ? proximaCuota.FechaVencimiento.ToString("dd/MM/yyyy") : null,

            Progreso = "No disponible todavía",
            UltimaSesion = "No disponible todavía",
            ProximaSesion = "No disponible todavía",
            CantidadEvoluciones = socio.Progresos.Count
        };

        return (true, dto, null, false);
    }
}
