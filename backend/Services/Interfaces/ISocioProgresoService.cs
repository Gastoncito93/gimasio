using Backend.DTOs.Progreso;

namespace Backend.Services.Interfaces;

public interface ISocioProgresoService
{
    Task<List<SocioProgresoDto>> GetProgresosBySocioAsync(int idSocio);
    Task<(bool Success, SocioProgresoDto? Data, string? Error)> CrearProgresoAsync(CrearSocioProgresoDto dto, string webRootPath);
    Task<(bool Success, string? Error)> EliminarProgresoAsync(int idProgreso, string webRootPath);
}
