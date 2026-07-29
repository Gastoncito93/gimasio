namespace Backend.DTOs.Auth;

using System.ComponentModel.DataAnnotations;

public class RegisterRequestDto
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio.")]
    public string Rol { get; set; } = "Alumno"; // "Alumno" o "Coach"

    // Campos específicos para Alumno
    public string? Dni { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public int? IdCoach { get; set; }
    public int? IdPlan { get; set; }

    // Campos específicos para Coach
    public int? IdActividad { get; set; }
}
