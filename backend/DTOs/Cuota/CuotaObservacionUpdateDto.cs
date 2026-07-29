namespace Backend.DTOs.Cuota;

using System.ComponentModel.DataAnnotations;

public class CuotaObservacionUpdateDto
{
    [MaxLength(255, ErrorMessage = "La observación no puede superar los 255 caracteres.")]
    public string? Observacion { get; set; }
}
