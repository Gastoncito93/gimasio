using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Auth;

public class CambiarPasswordPrimerIngresoDto
{
    [Required(ErrorMessage = "La contraseña actual es requerida.")]
    public string PasswordActual { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es requerida.")]
    [MinLength(6, ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres.")]
    public string NuevaPassword { get; set; } = string.Empty;
}
