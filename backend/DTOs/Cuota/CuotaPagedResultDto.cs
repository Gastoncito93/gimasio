namespace Backend.DTOs.Cuota;

using Backend.DTOs.Common;

public class CuotaPagedResultDto : PagedResultDto<CuotaResponseDto>
{
    public decimal TotalRecaudado { get; set; }
    public decimal TotalPendiente { get; set; }
    public int CantidadPagadas { get; set; }
    public int CantidadPendientes { get; set; }
}
