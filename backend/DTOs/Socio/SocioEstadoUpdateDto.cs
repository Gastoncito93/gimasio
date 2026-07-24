namespace Backend.DTOs.Socio;

using System.ComponentModel.DataAnnotations;

public class SocioEstadoUpdateDto
{
    [Required(ErrorMessage = "El estado es obligatorio.")]
    public string Estado { get; set; } = string.Empty;
}
