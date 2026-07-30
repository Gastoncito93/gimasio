namespace Backend.Services.Interfaces;

using Backend.DTOs.Common;
using Backend.DTOs.Cuota;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ICuotaService
{
    Task<CuotaPagedResultDto> GetPagedAsync(int page, int pageSize, string? search, string? estado, int? periodo);
    Task<CuotaResponseDto?> GetByIdAsync(int id);
    Task<(bool Success, CuotaResponseDto? Data, List<string> Errors)> CreateAsync(CuotaCreateDto dto);
    Task<(bool Success, CuotaResponseDto? Data, List<string> Errors, bool NotFound)> UpdateObservacionAsync(int id, CuotaObservacionUpdateDto dto);
    Task<(bool Success, CuotaResponseDto? Data, List<string> Errors, bool NotFound)> PagarAsync(int id, CuotaPagoDto dto);
    Task<(bool Success, CuotaResponseDto? Data, List<string> Errors, bool NotFound)> AnularAsync(int id);
}
