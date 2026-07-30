using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Auth;

public class RestablecerPasswordConCodigoDto
{
    [Required(ErrorMessage = "Debe ingresar el usuario o correo electrónico.")]
    public string EmailOrUsername { get; set; } = string.Empty;

    [Required(ErrorMessage = "El código de 6 dígitos es requerido.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "El código debe tener exactamente 6 dígitos.")]
    public string Codigo6Digitos { get; set; } = string.Empty;

    [Required(ErrorMessage = "La nueva contraseña es requerida.")]
    [MinLength(6, ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres.")]
    public string NuevaPassword { get; set; } = string.Empty;
}
