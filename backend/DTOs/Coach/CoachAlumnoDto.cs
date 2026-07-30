namespace Backend.DTOs.Coach;

using System;

public class CoachAlumnoDto
{
    public int Id { get; set; }
    public string Dni { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Avatar { get; set; }
    public string Estado { get; set; } = "Activo";
    public string PlanNombre { get; set; } = string.Empty;
    public string DeudaEstado { get; set; } = "Al día";
    public string Progreso { get; set; } = "No disponible todavía";
    public string UltimaSesion { get; set; } = "No disponible todavía";
    public string? Observaciones { get; set; }
    public string CoachNombre { get; set; } = "Sin asignación";
    public string ActividadNombre { get; set; } = "Sin asignación";
    public int CantidadEvoluciones { get; set; }
    public DateTime FechaAlta { get; set; }
}

public class CoachAlumnoDetalleDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Avatar { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string Estado { get; set; } = "Activo";
    public DateTime FechaAlta { get; set; }
    public string? Observaciones { get; set; }

    // Plan
    public int? IdPlan { get; set; }
    public string PlanNombre { get; set; } = "Sin plan";
    public decimal? PlanPrecio { get; set; }

    // Coach & Actividad
    public int? IdCoach { get; set; }
    public string CoachNombre { get; set; } = "Sin asignación";
    public string ActividadNombre { get; set; } = "Sin asignación";

    // Estado de Cuenta / Deuda
    public string DeudaEstado { get; set; } = "Al día";
    public int CuotasPendientesCount { get; set; }
    public string? ProximoVencimiento { get; set; }

    // Progreso & Sesiones
    public string Progreso { get; set; } = "No disponible todavía";
    public string UltimaSesion { get; set; } = "No disponible todavía";
    public string ProximaSesion { get; set; } = "No disponible todavía";
    public int CantidadEvoluciones { get; set; }
}
