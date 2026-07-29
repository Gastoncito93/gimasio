namespace Backend.DTOs.Cuota;

using System;
using System.ComponentModel.DataAnnotations;

public class CuotaCreateDto
{
    [Required(ErrorMessage = "El socio es obligatorio.")]
    public int IdSocio { get; set; }

    [Required(ErrorMessage = "El período es obligatorio.")]
    [Range(100000, 999999, ErrorMessage = "El período debe tener formato AAAAMM.")]
    public int Periodo { get; set; }

    [Required(ErrorMessage = "El monto es obligatorio.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que 0.")]
    public decimal Monto { get; set; }

    [Required(ErrorMessage = "La fecha de vencimiento es obligatoria.")]
    public DateTime FechaVencimiento { get; set; }

    [MaxLength(255, ErrorMessage = "La observación no puede superar los 255 caracteres.")]
    public string? Observacion { get; set; }
}
