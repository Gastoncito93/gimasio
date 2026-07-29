namespace Backend.Services.Business;

using Backend.Data;
using Backend.DTOs.Usuario;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class UsuarioService : IUsuarioService
{
    private readonly AppDbContext _context;

    public UsuarioService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CoachListDto>> GetCoachesListAsync(string? search = null)
    {
        var query = _context.Usuarios
            .Include(u => u.Actividad)
            .Where(u => u.IdRol == 2 && !u.EliminadoAt.HasValue);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTrim = search.Trim().ToLower();
            query = query.Where(u => u.Nombre.ToLower().Contains(searchTrim) || u.Username.ToLower().Contains(searchTrim));
        }

        var coaches = await query
            .OrderBy(u => u.Nombre)
            .ToListAsync();

        // Contar alumnos asignados a cada coach
        var result = new List<CoachListDto>();
        foreach (var c in coaches)
        {
            int cantAlumnos = await _context.Socios.CountAsync(s => s.IdCoach == c.Id);
            result.Add(new CoachListDto
            {
                Id = c.Id,
                Username = c.Username,
                Nombre = c.Nombre,
                RutaAvatar = c.RutaAvatar,
                IdActividad = c.IdActividad,
                ActividadNombre = c.Actividad?.Nombre,
                CantidadAlumnos = cantAlumnos
            });
        }

        return result;
    }

    public async Task<(bool Success, CoachListDto? Data, List<string> Errors)> CreateCoachAsync(CreateCoachDto dto)
    {
        var errors = new List<string>();
        var usernameTrim = dto.Username.Trim().ToLower();

        bool usernameExists = await _context.Usuarios.AnyAsync(u => u.Username.ToLower() == usernameTrim);
        if (usernameExists)
        {
            errors.Add("El nombre de usuario ya está registrado.");
            return (false, null, errors);
        }

        if (dto.IdActividad.HasValue)
        {
            bool actExists = await _context.Actividades.AnyAsync(a => a.Id == dto.IdActividad.Value);
            if (!actExists)
            {
                errors.Add("La actividad seleccionada no existe.");
                return (false, null, errors);
            }
        }

        var newCoach = new Usuario
        {
            Username = dto.Username.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Nombre = dto.Nombre.Trim(),
            IdRol = 2, // Coach
            IdActividad = dto.IdActividad
        };

        _context.Usuarios.Add(newCoach);
        await _context.SaveChangesAsync();

        var actObj = dto.IdActividad.HasValue
            ? await _context.Actividades.FindAsync(dto.IdActividad.Value)
            : null;

        var result = new CoachListDto
        {
            Id = newCoach.Id,
            Username = newCoach.Username,
            Nombre = newCoach.Nombre,
            RutaAvatar = newCoach.RutaAvatar,
            IdActividad = newCoach.IdActividad,
            ActividadNombre = actObj?.Nombre,
            CantidadAlumnos = 0
        };

        return (true, result, errors);
    }

    public async Task<(bool Success, CoachListDto? Data, List<string> Errors, bool NotFound)> UpdateCoachAsync(int id, UpdateCoachDto dto)
    {
        var errors = new List<string>();

        var coach = await _context.Usuarios
            .Include(u => u.Actividad)
            .FirstOrDefaultAsync(u => u.Id == id && u.IdRol == 2 && !u.EliminadoAt.HasValue);

        if (coach == null)
        {
            return (false, null, errors, true);
        }

        if (dto.IdActividad.HasValue)
        {
            bool actExists = await _context.Actividades.AnyAsync(a => a.Id == dto.IdActividad.Value);
            if (!actExists)
            {
                errors.Add("La actividad seleccionada no existe.");
                return (false, null, errors, false);
            }
        }

        coach.Nombre = dto.Nombre.Trim();
        coach.IdActividad = dto.IdActividad;

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            if (dto.Password.Length < 6)
            {
                errors.Add("La contraseña debe tener al menos 6 caracteres.");
                return (false, null, errors, false);
            }
            coach.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        await _context.SaveChangesAsync();

        var actObj = dto.IdActividad.HasValue
            ? await _context.Actividades.FindAsync(dto.IdActividad.Value)
            : null;

        int cantAlumnos = await _context.Socios.CountAsync(s => s.IdCoach == coach.Id);

        var result = new CoachListDto
        {
            Id = coach.Id,
            Username = coach.Username,
            Nombre = coach.Nombre,
            RutaAvatar = coach.RutaAvatar,
            IdActividad = coach.IdActividad,
            ActividadNombre = actObj?.Nombre,
            CantidadAlumnos = cantAlumnos
        };

        return (true, result, errors, false);
    }

    public async Task<(bool Success, List<string> Errors, bool NotFound)> DeleteCoachAsync(int id)
    {
        var errors = new List<string>();

        var coach = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id && u.IdRol == 2 && !u.EliminadoAt.HasValue);
        if (coach == null)
        {
            return (false, errors, true);
        }

        // Soft delete
        coach.EliminadoAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return (true, errors, false);
    }
}
