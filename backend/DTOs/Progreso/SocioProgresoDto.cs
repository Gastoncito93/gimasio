namespace Backend.DTOs.Progreso;

public class SocioProgresoDto
{
    public int Id { get; set; }
    public int IdSocio { get; set; }
    public DateTime Fecha { get; set; }
    public decimal? PesoKg { get; set; }
    public string? Observaciones { get; set; }
    public string? TipoRegistro { get; set; }
    public string? EjercicioNombre { get; set; }
    public decimal? ValorMetrica { get; set; }
    public string? UnidadMetrica { get; set; }
    public string? RutaFotoFrente { get; set; }
    public string? RutaFotoPerfil { get; set; }
    public string? RutaFotoEspalda { get; set; }
    public DateTime CreadoAt { get; set; }
}
