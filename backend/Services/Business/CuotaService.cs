namespace Backend.Services.Business;

using Backend.Data;
using Backend.DTOs.Common;
using Backend.DTOs.Cuota;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class CuotaService : ICuotaService
{
    private readonly AppDbContext _context;

    public CuotaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CuotaPagedResultDto> GetPagedAsync(int page, int pageSize, string? search, string? estado, int? periodo)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;

        IQueryable<Cuota> query = _context.Cuotas
            .Include(c => c.Socio)
            .ThenInclude(s => s.Usuario)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var sL = search.Trim().ToLower();
            query = query.Where(c => c.Socio.Dni.ToLower().Contains(sL) || 
                                     c.Socio.NombreCompleto.ToLower().Contains(sL));
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            var estadoTrim = estado.Trim();
            query = query.Where(c => c.Estado == estadoTrim);
        }

        if (periodo.HasValue)
        {
            query = query.Where(c => c.Periodo == periodo.Value);
        }

        int totalItems = await query.CountAsync();
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var totalRecaudado = await query
            .Where(c => c.Estado == "Pagada" || c.Estado == "Pagado")
            .SumAsync(c => (decimal?)c.Monto) ?? 0m;

        var totalPendiente = await query
            .Where(c => c.Estado == "Pendiente")
            .SumAsync(c => (decimal?)c.Monto) ?? 0m;

        var cantidadPagadas = await query
            .CountAsync(c => c.Estado == "Pagada" || c.Estado == "Pagado");

        var cantidadPendientes = await query
            .CountAsync(c => c.Estado == "Pendiente");

        var items = await query
            .OrderByDescending(c => c.Periodo)
            .ThenByDescending(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CuotaResponseDto
            {
                Id = c.Id,
                IdSocio = c.IdSocio,
                SocioNombreCompleto = c.Socio.NombreCompleto,
                SocioDni = c.Socio.Dni,
                SocioAvatar = c.Socio.Usuario != null ? c.Socio.Usuario.RutaAvatar : null,
                Periodo = c.Periodo,
                Monto = c.Monto,
                FechaVencimiento = c.FechaVencimiento,
                FechaPago = c.FechaPago,
                Estado = c.Estado,
                Observacion = c.Observacion
            })
            .ToListAsync();

        return new CuotaPagedResultDto
        {
            Data = items,
            Pagination = new PaginationMetadata
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            },
            TotalRecaudado = totalRecaudado,
            TotalPendiente = totalPendiente,
            CantidadPagadas = cantidadPagadas,
            CantidadPendientes = cantidadPendientes
        };
    }

    public async Task<CuotaResponseDto?> GetByIdAsync(int id)
    {
        var c = await _context.Cuotas
            .Include(c => c.Socio)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (c == null) return null;

        return new CuotaResponseDto
        {
            Id = c.Id,
            IdSocio = c.IdSocio,
            SocioNombreCompleto = c.Socio.NombreCompleto,
            SocioDni = c.Socio.Dni,
            Periodo = c.Periodo,
            Monto = c.Monto,
            FechaVencimiento = c.FechaVencimiento,
            FechaPago = c.FechaPago,
            Estado = c.Estado,
            Observacion = c.Observacion
        };
    }

    public async Task<(bool Success, CuotaResponseDto? Data, List<string> Errors)> CreateAsync(CuotaCreateDto dto)
    {
        var errors = new List<string>();

        // Verify Socio exists and is Active
        var socio = await _context.Socios.AsNoTracking().FirstOrDefaultAsync(s => s.Id == dto.IdSocio);
        if (socio == null)
        {
            errors.Add("El socio seleccionado no existe.");
            return (false, null, errors);
        }

        if (socio.Estado != "Activo")
        {
            errors.Add("No se puede crear una cuota para un socio inactivo.");
            return (false, null, errors);
        }

        // Calculate total already paid by this socio for this period
        var paidCuotas = await _context.Cuotas
            .Where(c => c.IdSocio == dto.IdSocio && c.Periodo == dto.Periodo && c.Estado == "Pagado")
            .ToListAsync();

        decimal totalPagado = paidCuotas.Sum(c => c.Monto);

        // Fetch socio's plan price to compare
        var plan = await _context.Planes.FirstOrDefaultAsync(p => p.Id == socio.IdPlan);
        decimal planPrecio = plan?.PrecioMensual ?? 0;

        if (planPrecio > 0 && totalPagado >= planPrecio)
        {
            errors.Add($"El socio {socio.NombreCompleto} ya cubrió el 100% de la cuota mensual de su plan (${totalPagado:N2}) para el período seleccionado.");
            return (false, null, errors);
        }

        // Clean up any non-paid (Pendiente or Anulado) cuotas for this socio in this period so the new cuota replaces it seamlessly
        var nonPaidCuotas = await _context.Cuotas
            .Where(c => c.IdSocio == dto.IdSocio && c.Periodo == dto.Periodo && c.Estado != "Pagado")
            .ToListAsync();

        if (nonPaidCuotas.Any())
        {
            _context.Cuotas.RemoveRange(nonPaidCuotas);
            await _context.SaveChangesAsync();
        }

        var cuota = new Cuota
        {
            IdSocio = dto.IdSocio,
            Periodo = dto.Periodo,
            Monto = dto.Monto,
            FechaVencimiento = dto.FechaVencimiento,
            Estado = "Pendiente",
            Observacion = string.IsNullOrWhiteSpace(dto.Observacion) ? null : dto.Observacion.Trim()
        };

        try
        {
            _context.Cuotas.Add(cuota);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            errors.Add("Error en la base de datos al guardar la cuota: " + (ex.InnerException?.Message ?? ex.Message));
            return (false, null, errors);
        }

        var response = await GetByIdAsync(cuota.Id);
        return (true, response, errors);
    }

    public async Task<(bool Success, CuotaResponseDto? Data, List<string> Errors, bool NotFound)> UpdateObservacionAsync(int id, CuotaObservacionUpdateDto dto)
    {
        var cuota = await _context.Cuotas.FirstOrDefaultAsync(c => c.Id == id);
        if (cuota == null)
        {
            return (false, null, new List<string>(), true);
        }

        cuota.Observacion = string.IsNullOrWhiteSpace(dto.Observacion) ? null : dto.Observacion.Trim();
        await _context.SaveChangesAsync();

        var response = await GetByIdAsync(cuota.Id);
        return (true, response, new List<string>(), false);
    }

    public async Task<(bool Success, CuotaResponseDto? Data, List<string> Errors, bool NotFound)> PagarAsync(int id, CuotaPagoDto dto)
    {
        var cuota = await _context.Cuotas.FirstOrDefaultAsync(c => c.Id == id);
        if (cuota == null)
        {
            return (false, null, new List<string>(), true);
        }

        var errors = new List<string>();

        if (cuota.Estado == "Pagado")
        {
            errors.Add("La cuota ya se encuentra pagada.");
            return (false, null, errors, false);
        }

        if (cuota.Estado == "Anulado")
        {
            errors.Add("No se puede registrar el pago de una cuota anulada.");
            return (false, null, errors, false);
        }

        cuota.Estado = "Pagado";
        cuota.FechaPago = dto.FechaPago;

        await _context.SaveChangesAsync();

        var response = await GetByIdAsync(cuota.Id);
        return (true, response, errors, false);
    }

    public async Task<(bool Success, CuotaResponseDto? Data, List<string> Errors, bool NotFound)> AnularAsync(int id)
    {
        var cuota = await _context.Cuotas.FirstOrDefaultAsync(c => c.Id == id);
        if (cuota == null)
        {
            return (false, null, new List<string>(), true);
        }

        var errors = new List<string>();

        if (cuota.Estado == "Anulado")
        {
            errors.Add("La cuota ya se encuentra anulada.");
            return (false, null, errors, false);
        }

        cuota.Estado = "Anulado";

        await _context.SaveChangesAsync();

        var response = await GetByIdAsync(cuota.Id);
        return (true, response, errors, false);
    }
}
