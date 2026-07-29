namespace Backend.Services.Interfaces;

using Backend.DTOs.Actividad;

public interface IActividadService
{
    Task<List<ActividadResponseDto>> GetAllAsync(string? search = null, string? estado = null);
    Task<ActividadResponseDto?> GetByIdAsync(int id);
    Task<(bool Success, ActividadResponseDto? Data, List<string> Errors)> CreateAsync(ActividadCreateUpdateDto dto);
    Task<(bool Success, ActividadResponseDto? Data, List<string> Errors, bool NotFound)> UpdateAsync(int id, ActividadCreateUpdateDto dto);
    Task<(bool Success, List<string> Errors, bool NotFound)> DeleteAsync(int id);
}
