namespace Backend.DTOs.Cuota;

using System;
using System.ComponentModel.DataAnnotations;

public class CuotaPagoDto
{
    [Required(ErrorMessage = "La fecha de pago es obligatoria.")]
    public DateTime FechaPago { get; set; }
}
