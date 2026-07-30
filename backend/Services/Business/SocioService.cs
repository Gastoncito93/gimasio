namespace Backend.Services.Business;

using Backend.Data;
using Backend.DTOs.Common;
using Backend.DTOs.Socio;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class SocioService : ISocioService
{
    private readonly AppDbContext _context;

    public SocioService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CoachSelectItemDto>> GetCoachesAsync()
    {
        var coaches = await _context.Usuarios
            .Include(u => u.Actividad)
            .Where(u => u.IdRol == 2 && u.EliminadoAt == null)
            .OrderBy(u => u.Nombre)
            .ToListAsync();

        var result = new List<CoachSelectItemDto>();
        foreach (var c in coaches)
        {
            int count = await _context.Socios.CountAsync(s => s.IdCoach == c.Id);
            result.Add(new CoachSelectItemDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Username = c.Username,
                ActividadNombre = c.Actividad != null ? c.Actividad.Nombre : "Musculación",
                AlumnosActuales = count,
                CupoMaximo = 20
            });
        }
        return result;
    }

    public async Task<PagedResultDto<SocioResponseDto>> GetPagedAsync(int page, int pageSize, string? search, string? estado, int? idPlan, int? idActividad = null)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;

        IQueryable<Socio> query = _context.Socios
            .Include(s => s.Plan)
            .Include(s => s.Coach).ThenInclude(c => c!.Actividad)
            .Include(s => s.Usuario)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var sL = search.Trim().ToLower();
            query = query.Where(s => s.Dni.ToLower().Contains(sL) || 
                                     s.NombreCompleto.ToLower().Contains(sL) || 
                                     (s.Telefono != null && s.Telefono.ToLower().Contains(sL)) || 
                                     (s.Email != null && s.Email.ToLower().Contains(sL)));
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            var estadoTrim = estado.Trim();
            query = query.Where(s => s.Estado == estadoTrim);
        }

        if (idPlan.HasValue)
        {
            query = query.Where(s => s.IdPlan == idPlan.Value);
        }

        if (idActividad.HasValue)
        {
            query = query.Where(s => s.Coach != null && s.Coach.IdActividad == idActividad.Value);
        }

        int totalItems = await query.CountAsync();
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = await query
            .OrderByDescending(s => s.FechaAlta)
            .ThenByDescending(s => s.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SocioResponseDto
            {
                Id = s.Id,
                Dni = s.Dni,
                NombreCompleto = s.NombreCompleto,
                Telefono = s.Telefono,
                Email = s.Email,
                FechaAlta = s.FechaAlta,
                Estado = s.Estado,
                IdPlan = s.IdPlan,
                PlanNombre = s.Plan != null ? s.Plan.Nombre : "Sin plan",
                IdCoach = s.IdCoach,
                CoachNombre = s.Coach != null ? s.Coach.Nombre : null,
                ActividadNombre = s.Coach != null && s.Coach.Actividad != null ? s.Coach.Actividad.Nombre : "Sin asignación",
                Avatar = s.Usuario != null ? s.Usuario.RutaAvatar : null,
                Observacion = s.Observacion
            })
            .ToListAsync();

        return new PagedResultDto<SocioResponseDto>
        {
            Data = items,
            Pagination = new PaginationMetadata
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            }
        };
    }

    public async Task<SocioResponseDto?> GetByIdAsync(int id)
    {
        var socio = await _context.Socios
            .Include(s => s.Plan)
            .Include(s => s.Coach).ThenInclude(c => c!.Actividad)
            .Include(s => s.Usuario)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (socio == null) return null;

        return new SocioResponseDto
        {
            Id = socio.Id,
            Dni = socio.Dni,
            NombreCompleto = socio.NombreCompleto,
            Telefono = socio.Telefono,
            Email = socio.Email,
            FechaAlta = socio.FechaAlta,
            Estado = socio.Estado,
            IdPlan = socio.IdPlan,
            PlanNombre = socio.Plan != null ? socio.Plan.Nombre : "Sin plan",
            IdCoach = socio.IdCoach,
            CoachNombre = socio.Coach != null ? socio.Coach.Nombre : null,
            ActividadNombre = socio.Coach != null && socio.Coach.Actividad != null ? socio.Coach.Actividad.Nombre : "Sin asignación",
            Avatar = socio.Usuario != null ? socio.Usuario.RutaAvatar : null,
            Observacion = socio.Observacion
        };
    }

    public async Task<SocioResponseDto?> GetByUsuarioIdAsync(int userId)
    {
        var socio = await _context.Socios
            .Include(s => s.Plan)
            .Include(s => s.Coach).ThenInclude(c => c!.Actividad)
            .Include(s => s.Usuario)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.IdUsuario == userId);

        if (socio == null) return null;

        return new SocioResponseDto
        {
            Id = socio.Id,
            Dni = socio.Dni,
            NombreCompleto = socio.NombreCompleto,
            Telefono = socio.Telefono,
            Email = socio.Email,
            FechaAlta = socio.FechaAlta,
            Estado = socio.Estado,
            IdPlan = socio.IdPlan,
            PlanNombre = socio.Plan != null ? socio.Plan.Nombre : string.Empty,
            IdCoach = socio.IdCoach,
            CoachNombre = socio.Coach != null ? socio.Coach.Nombre : null,
            ActividadNombre = socio.Coach != null && socio.Coach.Actividad != null ? socio.Coach.Actividad.Nombre : "Musculación",
            Avatar = socio.Usuario != null ? socio.Usuario.RutaAvatar : null,
            Observacion = socio.Observacion
        };
    }

    public async Task<IEnumerable<SocioSearchResultDto>> BuscarAsync(string q, int limit)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 3)
        {
            return Enumerable.Empty<SocioSearchResultDto>();
        }
        limit = limit < 1 ? 10 : limit;
        var sL = q.Trim().ToLower();

        return await _context.Socios
            .Include(s => s.Plan)
            .AsNoTracking()
            .Where(s => s.Dni.ToLower().Contains(sL) || s.NombreCompleto.ToLower().Contains(sL))
            .OrderBy(s => s.Id)
            .Take(limit)
            .Select(s => new SocioSearchResultDto
            {
                Id = s.Id,
                Dni = s.Dni,
                NombreCompleto = s.NombreCompleto,
                Estado = s.Estado,
                IdPlan = s.IdPlan ?? 0,
                PlanNombre = s.Plan != null ? s.Plan.Nombre : "Sin plan",
                PlanPrecio = s.Plan != null ? s.Plan.PrecioMensual : 0
            })
            .ToListAsync();
    }

    public async Task<(bool Success, SocioResponseDto? Data, List<string> Errors)> CreateAsync(SocioCreateUpdateDto dto)
    {
        var errors = await ValidateSocioAsync(dto);
        if (errors.Count > 0)
        {
            return (false, null, errors);
        }

        var socio = new Socio
        {
            Dni = dto.Dni.Trim(),
            NombreCompleto = dto.NombreCompleto.Trim(),
            Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim(),
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
            FechaAlta = dto.FechaAlta,
            Estado = string.IsNullOrWhiteSpace(dto.Estado) ? "Activo" : dto.Estado.Trim(),
            IdPlan = dto.IdPlan,
            IdCoach = dto.IdCoach,
            Observacion = string.IsNullOrWhiteSpace(dto.Observacion) ? null : dto.Observacion.Trim()
        };

        _context.Socios.Add(socio);
        await _context.SaveChangesAsync();

        var response = await GetByIdAsync(socio.Id);
        return (true, response, errors);
    }

    public async Task<(bool Success, SocioResponseDto? Data, List<string> Errors, bool NotFound)> UpdateAsync(int id, SocioCreateUpdateDto dto)
    {
        var socio = await _context.Socios.FirstOrDefaultAsync(s => s.Id == id);
        if (socio == null)
        {
            return (false, null, new List<string>(), true);
        }

        var errors = await ValidateSocioAsync(dto, id);
        if (errors.Count > 0)
        {
            return (false, null, errors, false);
        }

        socio.Dni = dto.Dni.Trim();
        socio.NombreCompleto = dto.NombreCompleto.Trim();
        socio.Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim();
        socio.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
        socio.FechaAlta = dto.FechaAlta;
        socio.Estado = string.IsNullOrWhiteSpace(dto.Estado) ? "Activo" : dto.Estado.Trim();
        socio.IdPlan = dto.IdPlan;
        socio.IdCoach = dto.IdCoach;
        socio.Observacion = string.IsNullOrWhiteSpace(dto.Observacion) ? null : dto.Observacion.Trim();

        await _context.SaveChangesAsync();

        var response = await GetByIdAsync(socio.Id);
        return (true, response, errors, false);
    }

    public async Task<(bool Success, SocioResponseDto? Data, List<string> Errors, bool NotFound)> ChangeEstadoAsync(int id, string estado)
    {
        var socio = await _context.Socios.FirstOrDefaultAsync(s => s.Id == id);
        if (socio == null)
        {
            return (false, null, new List<string>(), true);
        }

        var errors = new List<string>();
        var estadoTrim = estado?.Trim();
        if (estadoTrim != "Activo" && estadoTrim != "Inactivo")
        {
            errors.Add("El estado solo puede ser 'Activo' o 'Inactivo'.");
            return (false, null, errors, false);
        }

        socio.Estado = estadoTrim;
        await _context.SaveChangesAsync();

        var response = await GetByIdAsync(socio.Id);
        return (true, response, errors, false);
    }

    private async Task<List<string>> ValidateSocioAsync(SocioCreateUpdateDto dto, int? currentId = null)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.Dni))
        {
            errors.Add("El DNI es obligatorio.");
        }
        else
        {
            var dniTrim = dto.Dni.Trim().ToLower();
            bool exists = await _context.Socios
                .AnyAsync(s => (currentId == null || s.Id != currentId) && s.Dni.ToLower() == dniTrim);
            if (exists)
            {
                errors.Add("El DNI ya se encuentra registrado por otro socio.");
            }
        }

        if (string.IsNullOrWhiteSpace(dto.NombreCompleto))
        {
            errors.Add("El nombre completo es obligatorio.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(dto.Email.Trim());
                if (addr.Address != dto.Email.Trim())
                {
                    errors.Add("El formato de correo electrónico no es válido.");
                }
            }
            catch
            {
                errors.Add("El formato de correo electrónico no es válido.");
            }
        }

        if (dto.IdPlan.HasValue)
        {
            var plan = await _context.Planes.AsNoTracking().FirstOrDefaultAsync(p => p.Id == dto.IdPlan.Value);
            if (plan == null)
            {
                errors.Add("El plan seleccionado no existe.");
            }
            else if (plan.Estado != "Activo")
            {
                bool isCurrentPlan = currentId.HasValue && await _context.Socios.AnyAsync(s => s.Id == currentId.Value && s.IdPlan == dto.IdPlan.Value);
                if (!isCurrentPlan)
                {
                    errors.Add("Solo se puede asignar un plan que esté en estado 'Activo'.");
                }
            }
        }

        if (dto.IdCoach.HasValue)
        {
            var coachExists = await _context.Usuarios.AsNoTracking().AnyAsync(u => u.Id == dto.IdCoach.Value && u.IdRol == 2 && u.EliminadoAt == null);
            if (!coachExists)
            {
                errors.Add("El coach seleccionado no existe o no tiene el rol correspondiente.");
            }
            else
            {
                int cantAlumnos = await _context.Socios.CountAsync(s => s.IdCoach == dto.IdCoach.Value && (currentId == null || s.Id != currentId.Value));
                if (cantAlumnos >= 20)
                {
                    errors.Add("El coach seleccionado ha alcanzado el cupo máximo de 20 alumnos.");
                }
            }
        }

        var estadoTrim = dto.Estado?.Trim();
        if (estadoTrim != "Activo" && estadoTrim != "Inactivo")
        {
            errors.Add("El estado solo puede ser 'Activo' o 'Inactivo'.");
        }

        if (dto.FechaAlta == default)
        {
            errors.Add("La fecha de alta es obligatoria.");
        }

        return errors;
    }
}
