namespace Backend.Services.Business;

using Backend.Data;
using Backend.DTOs.Actividad;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class ActividadService : IActividadService
{
    private readonly AppDbContext _context;

    public ActividadService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ActividadResponseDto>> GetAllAsync(string? search = null, string? estado = null)
    {
        IQueryable<Actividad> query = _context.Actividades
            .Include(a => a.Coaches.Where(c => c.EliminadoAt == null))
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var sL = search.Trim().ToLower();
            query = query.Where(a => a.Nombre.ToLower().Contains(sL) || (a.Descripcion != null && a.Descripcion.ToLower().Contains(sL)));
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            var estTrim = estado.Trim();
            query = query.Where(a => a.Estado == estTrim);
        }

        var list = await query.OrderBy(a => a.Nombre).ToListAsync();

        return list.Select(a => new ActividadResponseDto
        {
            Id = a.Id,
            Nombre = a.Nombre,
            Descripcion = a.Descripcion,
            Estado = a.Estado,
            CreadoAt = a.CreadoAt,
            CantidadCoaches = a.Coaches.Count,
            NombresCoaches = a.Coaches.Select(c => c.Nombre).ToList()
        }).ToList();
    }

    public async Task<ActividadResponseDto?> GetByIdAsync(int id)
    {
        var actividad = await _context.Actividades
            .Include(a => a.Coaches.Where(c => c.EliminadoAt == null))
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (actividad == null) return null;

        return new ActividadResponseDto
        {
            Id = actividad.Id,
            Nombre = actividad.Nombre,
            Descripcion = actividad.Descripcion,
            Estado = actividad.Estado,
            CreadoAt = actividad.CreadoAt,
            CantidadCoaches = actividad.Coaches.Count,
            NombresCoaches = actividad.Coaches.Select(c => c.Nombre).ToList()
        };
    }

    public async Task<(bool Success, ActividadResponseDto? Data, List<string> Errors)> CreateAsync(ActividadCreateUpdateDto dto)
    {
        var errors = await ValidateActividadAsync(dto);
        if (errors.Count > 0)
        {
            return (false, null, errors);
        }

        var actividad = new Actividad
        {
            Nombre = dto.Nombre.Trim(),
            Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
            Estado = string.IsNullOrWhiteSpace(dto.Estado) ? "Activo" : dto.Estado.Trim(),
            CreadoAt = DateTime.UtcNow
        };

        _context.Actividades.Add(actividad);
        await _context.SaveChangesAsync();

        var result = await GetByIdAsync(actividad.Id);
        return (true, result, errors);
    }

    public async Task<(bool Success, ActividadResponseDto? Data, List<string> Errors, bool NotFound)> UpdateAsync(int id, ActividadCreateUpdateDto dto)
    {
        var actividad = await _context.Actividades.FirstOrDefaultAsync(a => a.Id == id);
        if (actividad == null)
        {
            return (false, null, new List<string>(), true);
        }

        var errors = await ValidateActividadAsync(dto, id);
        if (errors.Count > 0)
        {
            return (false, null, errors, false);
        }

        actividad.Nombre = dto.Nombre.Trim();
        actividad.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();
        actividad.Estado = string.IsNullOrWhiteSpace(dto.Estado) ? "Activo" : dto.Estado.Trim();

        await _context.SaveChangesAsync();

        var result = await GetByIdAsync(actividad.Id);
        return (true, result, errors, false);
    }

    public async Task<(bool Success, List<string> Errors, bool NotFound)> DeleteAsync(int id)
    {
        var actividad = await _context.Actividades
            .Include(a => a.Coaches)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (actividad == null)
        {
            return (false, new List<string>(), true);
        }

        // Si la actividad tiene coaches asignados, la marcamos como "Inactivo" en lugar de eliminar el registro físico
        if (actividad.Coaches.Any())
        {
            actividad.Estado = "Inactivo";
            await _context.SaveChangesAsync();
            return (true, new List<string>(), false);
        }

        _context.Actividades.Remove(actividad);
        await _context.SaveChangesAsync();
        return (true, new List<string>(), false);
    }

    private async Task<List<string>> ValidateActividadAsync(ActividadCreateUpdateDto dto, int? currentId = null)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.Nombre))
        {
            errors.Add("El nombre de la actividad es obligatorio.");
        }
        else
        {
            var nombreTrim = dto.Nombre.Trim().ToLower();
            bool exists = await _context.Actividades
                .AnyAsync(a => (currentId == null || a.Id != currentId) && a.Nombre.ToLower() == nombreTrim);
            if (exists)
            {
                errors.Add("Ya existe una actividad registrada con ese nombre.");
            }
        }

        var estadoTrim = dto.Estado?.Trim();
        if (estadoTrim != "Activo" && estadoTrim != "Inactivo")
        {
            errors.Add("El estado solo puede ser 'Activo' o 'Inactivo'.");
        }

        return errors;
    }
}
