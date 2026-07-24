namespace Backend.Services.Interfaces;

using Backend.DTOs.Common;
using Backend.DTOs.Plan;

public interface IPlanService
{
    Task<PagedResultDto<PlanResponseDto>> GetPagedAsync(int page, int pageSize, string? search);
    Task<PlanResponseDto?> GetByIdAsync(int id);
    Task<(bool Success, PlanResponseDto? Data, List<string> Errors)> CreateAsync(PlanCreateUpdateDto dto);
    Task<(bool Success, PlanResponseDto? Data, List<string> Errors, bool NotFound)> UpdateAsync(int id, PlanCreateUpdateDto dto);
    Task<(bool Success, PlanResponseDto? Data, List<string> Errors, bool NotFound)> ChangeEstadoAsync(int id, PlanEstadoUpdateDto dto);
    Task<IEnumerable<PlanSearchResultDto>> BuscarAsync(string q, int limit);
}
