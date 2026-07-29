namespace Backend.DTOs.Actividad;

using System;
using System.Collections.Generic;

public class ActividadResponseDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Estado { get; set; } = "Activo";
    public DateTime CreadoAt { get; set; }
    public int CantidadCoaches { get; set; }
    public List<string> NombresCoaches { get; set; } = new List<string>();
}

public class ActividadCreateUpdateDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Estado { get; set; } = "Activo";
}
