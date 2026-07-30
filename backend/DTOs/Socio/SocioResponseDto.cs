namespace Backend.DTOs.Socio;

using System;

public class SocioResponseDto
{
    public int Id { get; set; }
    public string Dni { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public DateTime FechaAlta { get; set; }
    public string Estado { get; set; } = string.Empty;
    public int? IdPlan { get; set; }
    public string PlanNombre { get; set; } = "Sin plan";
    public int? IdCoach { get; set; }
    public string? CoachNombre { get; set; }
    public string ActividadNombre { get; set; } = "Sin asignación";
    public string? Avatar { get; set; }
    public string? Observacion { get; set; }
}
