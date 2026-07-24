namespace Backend.Services.Business;

using Backend.Data;
using Backend.DTOs.Common;
using Backend.DTOs.Plan;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class PlanService : IPlanService
{
    private readonly AppDbContext _context;

    public PlanService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResultDto<PlanResponseDto>> GetPagedAsync(int page, int pageSize, string? search)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;

        IQueryable<Plan> query = _context.Planes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(p => p.Nombre.ToLower().Contains(searchLower) ||
                                     (p.Descripcion != null && p.Descripcion.ToLower().Contains(searchLower)));
        }

        int totalItems = await query.CountAsync();
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var items = await query
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => MapToDto(p))
            .ToListAsync();

        return new PagedResultDto<PlanResponseDto>
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

    public async Task<PlanResponseDto?> GetByIdAsync(int id)
    {
        var plan = await _context.Planes.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return plan == null ? null : MapToDto(plan);
    }

    public async Task<(bool Success, PlanResponseDto? Data, List<string> Errors)> CreateAsync(PlanCreateUpdateDto dto)
    {
        var errors = await ValidatePlanAsync(dto);
        if (errors.Count > 0)
        {
            return (false, null, errors);
        }

        var plan = new Plan
        {
            Nombre = dto.Nombre.Trim(),
            Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim(),
            PrecioMensual = dto.PrecioMensual,
            Estado = string.IsNullOrWhiteSpace(dto.Estado) ? "Activo" : dto.Estado.Trim()
        };

        _context.Planes.Add(plan);
        await _context.SaveChangesAsync();

        return (true, MapToDto(plan), errors);
    }

    public async Task<(bool Success, PlanResponseDto? Data, List<string> Errors, bool NotFound)> UpdateAsync(int id, PlanCreateUpdateDto dto)
    {
        var plan = await _context.Planes.FirstOrDefaultAsync(p => p.Id == id);
        if (plan == null)
        {
            return (false, null, new List<string>(), true);
        }

        var errors = await ValidatePlanAsync(dto, id);
        if (errors.Count > 0)
        {
            return (false, null, errors, false);
        }

        plan.Nombre = dto.Nombre.Trim();
        plan.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();
        plan.PrecioMensual = dto.PrecioMensual;
        plan.Estado = string.IsNullOrWhiteSpace(dto.Estado) ? "Activo" : dto.Estado.Trim();

        await _context.SaveChangesAsync();

        return (true, MapToDto(plan), errors, false);
    }

    public async Task<(bool Success, PlanResponseDto? Data, List<string> Errors, bool NotFound)> ChangeEstadoAsync(int id, PlanEstadoUpdateDto dto)
    {
        var plan = await _context.Planes.FirstOrDefaultAsync(p => p.Id == id);
        if (plan == null)
        {
            return (false, null, new List<string>(), true);
        }

        var errors = new List<string>();
        var estadoTrim = dto.Estado?.Trim();
        if (estadoTrim != "Activo" && estadoTrim != "Inactivo")
        {
            errors.Add("El estado solo puede ser 'Activo' o 'Inactivo'.");
            return (false, null, errors, false);
        }

        plan.Estado = estadoTrim;
        await _context.SaveChangesAsync();

        return (true, MapToDto(plan), errors, false);
    }

    private async Task<List<string>> ValidatePlanAsync(PlanCreateUpdateDto dto, int? currentId = null)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.Nombre))
        {
            errors.Add("El nombre del plan es obligatorio.");
        }
        else if (dto.Nombre.Trim().Length > 100)
        {
            errors.Add("El nombre no puede superar los 100 caracteres.");
        }

        if (dto.PrecioMensual <= 0)
        {
            errors.Add("El precio mensual debe ser mayor a 0.");
        }

        var estadoTrim = dto.Estado?.Trim();
        if (estadoTrim != "Activo" && estadoTrim != "Inactivo")
        {
            errors.Add("El estado solo puede ser 'Activo' o 'Inactivo'.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Nombre))
        {
            var nombreLower = dto.Nombre.Trim().ToLower();
            bool nameExists = await _context.Planes
                .AnyAsync(p => (currentId == null || p.Id != currentId) && p.Nombre.ToLower() == nombreLower);

            if (nameExists)
            {
                errors.Add("No se permite crear dos planes con el mismo nombre.");
            }
        }

        return errors;
    }

    public async Task<IEnumerable<PlanSearchResultDto>> BuscarAsync(string q, int limit)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
        {
            return Enumerable.Empty<PlanSearchResultDto>();
        }

        limit = limit < 1 ? 10 : limit;
        var searchLower = q.Trim().ToLower();

        return await _context.Planes
            .AsNoTracking()
            .Where(p => p.Estado == "Activo" && p.Nombre.ToLower().Contains(searchLower))
            .OrderBy(p => p.Id)
            .Take(limit)
            .Select(p => new PlanSearchResultDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                PrecioMensual = p.PrecioMensual
            })
            .ToListAsync();
    }

    private static PlanResponseDto MapToDto(Plan plan)
    {
        return new PlanResponseDto
        {
            Id = plan.Id,
            Nombre = plan.Nombre,
            Descripcion = plan.Descripcion,
            PrecioMensual = plan.PrecioMensual,
            Estado = plan.Estado
        };
    }
}
