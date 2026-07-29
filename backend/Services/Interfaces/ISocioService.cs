namespace Backend.Services.Interfaces;

using Backend.DTOs.Common;
using Backend.DTOs.Socio;
using System.Collections.Generic;
using System.Threading.Tasks;

public class CoachSelectItemDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int AlumnosActuales { get; set; }
    public int CupoMaximo { get; set; } = 20;
    public bool CupoCompleto => AlumnosActuales >= CupoMaximo;
}

public interface ISocioService
{
    Task<PagedResultDto<SocioResponseDto>> GetPagedAsync(int page, int pageSize, string? search, string? estado, int? idPlan);
    Task<SocioResponseDto?> GetByIdAsync(int id);
    Task<SocioResponseDto?> GetByUsuarioIdAsync(int userId);
    Task<IEnumerable<SocioSearchResultDto>> BuscarAsync(string q, int limit);
    Task<List<CoachSelectItemDto>> GetCoachesAsync();
    Task<(bool Success, SocioResponseDto? Data, List<string> Errors)> CreateAsync(SocioCreateUpdateDto dto);
    Task<(bool Success, SocioResponseDto? Data, List<string> Errors, bool NotFound)> UpdateAsync(int id, SocioCreateUpdateDto dto);
    Task<(bool Success, SocioResponseDto? Data, List<string> Errors, bool NotFound)> ChangeEstadoAsync(int id, string estado);
}
