using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Auth;

public class SolicitarRecuperacionDto
{
    [Required(ErrorMessage = "Debe ingresar un usuario o correo electrónico.")]
    public string EmailOrUsername { get; set; } = string.Empty;
}
