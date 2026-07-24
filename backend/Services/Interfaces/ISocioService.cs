namespace Backend.Services.Interfaces;

using Backend.DTOs.Common;
using Backend.DTOs.Socio;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ISocioService
{
    Task<PagedResultDto<SocioResponseDto>> GetPagedAsync(int page, int pageSize, string? search, string? estado, int? idPlan);
    Task<SocioResponseDto?> GetByIdAsync(int id);
    Task<IEnumerable<SocioSearchResultDto>> BuscarAsync(string q, int limit);
    Task<(bool Success, SocioResponseDto? Data, List<string> Errors)> CreateAsync(SocioCreateUpdateDto dto);
    Task<(bool Success, SocioResponseDto? Data, List<string> Errors, bool NotFound)> UpdateAsync(int id, SocioCreateUpdateDto dto);
    Task<(bool Success, SocioResponseDto? Data, List<string> Errors, bool NotFound)> ChangeEstadoAsync(int id, string estado);
}
