namespace Backend.DTOs.Cuota;

using System;

public class CuotaResponseDto
{
    public int Id { get; set; }
    public int IdSocio { get; set; }
    public string SocioNombreCompleto { get; set; } = string.Empty;
    public string SocioDni { get; set; } = string.Empty;
    public int Periodo { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public DateTime? FechaPago { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public string? Observacion { get; set; }
}
