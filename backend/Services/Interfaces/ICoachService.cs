namespace Backend.Services.Interfaces;

using Backend.DTOs.Coach;

public interface ICoachService
{
    Task<List<CoachAlumnoDto>> GetMisAlumnosAsync(int userId, string userRole, string? search = null);
    Task<(bool Success, CoachAlumnoDetalleDto? Data, string? Error, bool IsForbidden)> GetAlumnoDetalleAsync(int alumnoId, int requestingUserId, string requestingUserRole);
}
