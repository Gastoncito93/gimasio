using Backend.Data;
using Backend.DTOs.Progreso;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Business;

public class SocioProgresoService : ISocioProgresoService
{
    private readonly AppDbContext _context;

    public SocioProgresoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SocioProgresoDto>> GetProgresosBySocioAsync(int idSocio)
    {
        return await _context.SociosProgresos
            .Where(sp => sp.IdSocio == idSocio)
            .OrderByDescending(sp => sp.Fecha)
            .ThenByDescending(sp => sp.CreadoAt)
            .Select(sp => new SocioProgresoDto
            {
                Id = sp.Id,
                IdSocio = sp.IdSocio,
                Fecha = sp.Fecha,
                PesoKg = sp.PesoKg,
                Observaciones = sp.Observaciones,
                RutaFotoFrente = sp.RutaFotoFrente,
                RutaFotoPerfil = sp.RutaFotoPerfil,
                RutaFotoEspalda = sp.RutaFotoEspalda,
                CreadoAt = sp.CreadoAt
            })
            .ToListAsync();
    }

    public async Task<(bool Success, SocioProgresoDto? Data, string? Error)> CrearProgresoAsync(CrearSocioProgresoDto dto, string webRootPath)
    {
        var socioExists = await _context.Socios.AnyAsync(s => s.Id == dto.IdSocio);
        if (!socioExists)
        {
            return (false, null, "El socio especificado no existe.");
        }

        var uploadsFolder = Path.Combine(webRootPath, "uploads", "progreso");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var (frenteSuccess, rutaFrente, errorFrente) = await SaveFileAsync(dto.FotoFrente, uploadsFolder);
        if (!frenteSuccess) return (false, null, errorFrente);

        var (perfilSuccess, rutaPerfil, errorPerfil) = await SaveFileAsync(dto.FotoPerfil, uploadsFolder);
        if (!perfilSuccess) return (false, null, errorPerfil);

        var (espaldaSuccess, rutaEspalda, errorEspalda) = await SaveFileAsync(dto.FotoEspalda, uploadsFolder);
        if (!espaldaSuccess) return (false, null, errorEspalda);

        var progreso = new SocioProgreso
        {
            IdSocio = dto.IdSocio,
            Fecha = dto.Fecha ?? DateTime.UtcNow,
            PesoKg = dto.PesoKg,
            Observaciones = dto.Observaciones?.Trim(),
            RutaFotoFrente = rutaFrente,
            RutaFotoPerfil = rutaPerfil,
            RutaFotoEspalda = rutaEspalda,
            CreadoAt = DateTime.UtcNow
        };

        _context.SociosProgresos.Add(progreso);
        await _context.SaveChangesAsync();

        var resultDto = new SocioProgresoDto
        {
            Id = progreso.Id,
            IdSocio = progreso.IdSocio,
            Fecha = progreso.Fecha,
            PesoKg = progreso.PesoKg,
            Observaciones = progreso.Observaciones,
            RutaFotoFrente = progreso.RutaFotoFrente,
            RutaFotoPerfil = progreso.RutaFotoPerfil,
            RutaFotoEspalda = progreso.RutaFotoEspalda,
            CreadoAt = progreso.CreadoAt
        };

        return (true, resultDto, null);
    }

    public async Task<(bool Success, string? Error)> EliminarProgresoAsync(int idProgreso, string webRootPath)
    {
        var progreso = await _context.SociosProgresos.FirstOrDefaultAsync(sp => sp.Id == idProgreso);
        if (progreso == null)
        {
            return (false, "El registro de progreso no existe.");
        }

        var uploadsFolder = Path.Combine(webRootPath, "uploads", "progreso");

        DeleteFileIfExists(progreso.RutaFotoFrente, uploadsFolder);
        DeleteFileIfExists(progreso.RutaFotoPerfil, uploadsFolder);
        DeleteFileIfExists(progreso.RutaFotoEspalda, uploadsFolder);

        _context.SociosProgresos.Remove(progreso);
        await _context.SaveChangesAsync();

        return (true, null);
    }

    private static async Task<(bool Success, string? FilePath, string? Error)> SaveFileAsync(IFormFile? file, string uploadsFolder)
    {
        if (file == null || file.Length == 0)
        {
            return (true, null, null);
        }

        const long maxSizeBytes = 5 * 1024 * 1024;
        if (file.Length > maxSizeBytes)
        {
            return (false, null, "El tamaño de cada imagen no puede superar 5 MB.");
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
        {
            return (false, null, "Formato de imagen no permitido. Formatos aceptados: JPG, JPEG, PNG, WEBP.");
        }

        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
        var destinationPath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(destinationPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return (true, $"/uploads/progreso/{uniqueFileName}", null);
    }

    private static void DeleteFileIfExists(string? relativePath, string uploadsFolder)
    {
        if (string.IsNullOrEmpty(relativePath) || !relativePath.StartsWith("/uploads/progreso/")) return;

        var fileName = Path.GetFileName(relativePath);
        var fullPath = Path.Combine(uploadsFolder, fileName);
        if (File.Exists(fullPath))
        {
            try
            {
                File.Delete(fullPath);
            }
            catch
            {
                // Ignore file locks during delete
            }
        }
    }
}
