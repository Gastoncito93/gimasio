namespace Backend.Services.Interfaces;

using Backend.DTOs.Usuario;

public interface IUsuarioService
{
    Task<List<CoachListDto>> GetCoachesListAsync(string? search = null);
    Task<(bool Success, CoachListDto? Data, List<string> Errors)> CreateCoachAsync(CreateCoachDto dto);
    Task<(bool Success, CoachListDto? Data, List<string> Errors, bool NotFound)> UpdateCoachAsync(int id, UpdateCoachDto dto);
    Task<(bool Success, List<string> Errors, bool NotFound)> DeleteCoachAsync(int id);
}
