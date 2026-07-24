namespace Backend.DTOs.Socio;

using System;
using System.ComponentModel.DataAnnotations;

public class SocioCreateUpdateDto
{
    [Required(ErrorMessage = "El DNI es obligatorio.")]
    public string Dni { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    public string NombreCompleto { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    [EmailAddress(ErrorMessage = "El formato de correo electrónico no es válido.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "La fecha de alta es obligatoria.")]
    public DateTime FechaAlta { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio.")]
    public string Estado { get; set; } = "Activo";

    [Required(ErrorMessage = "El plan es obligatorio.")]
    public int IdPlan { get; set; }
}
