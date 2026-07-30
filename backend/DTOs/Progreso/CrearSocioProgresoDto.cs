using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Backend.DTOs.Progreso;

public class CrearSocioProgresoDto
{
    [Required]
    public int IdSocio { get; set; }

    public DateTime? Fecha { get; set; }

    public decimal? PesoKg { get; set; }

    public string? Observaciones { get; set; }

    public string? TipoRegistro { get; set; }
    public string? EjercicioNombre { get; set; }
    public decimal? ValorMetrica { get; set; }
    public string? UnidadMetrica { get; set; }

    public IFormFile? FotoFrente { get; set; }
    public IFormFile? FotoPerfil { get; set; }
    public IFormFile? FotoEspalda { get; set; }
}
