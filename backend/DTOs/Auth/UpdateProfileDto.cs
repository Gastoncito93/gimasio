namespace Backend.DTOs.Auth;

using System.ComponentModel.DataAnnotations;

public class UpdateProfileDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;
}
