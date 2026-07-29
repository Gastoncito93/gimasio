namespace Backend.DTOs.Usuario;

using System.ComponentModel.DataAnnotations;

public class CoachListDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? RutaAvatar { get; set; }
    public int? IdActividad { get; set; }
    public string? ActividadNombre { get; set; }
    public int CantidadAlumnos { get; set; }
    public int CupoMaximo { get; set; } = 20;
    public bool CupoCompleto => CantidadAlumnos >= CupoMaximo;
}

public class CreateCoachDto
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;

    public int? IdActividad { get; set; }
}

public class UpdateCoachDto
{
    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;

    public string? Password { get; set; }

    public int? IdActividad { get; set; }
}
