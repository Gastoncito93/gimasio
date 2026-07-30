namespace Backend.Models;

public class SocioProgreso
{
    public int Id { get; set; }

    public int IdSocio { get; set; }
    public Socio Socio { get; set; } = null!;

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    public decimal? PesoKg { get; set; }
    public string? Observaciones { get; set; }

    // Campos extensibles por disciplina (Crossfit PRs, Spinning Cardio, Yoga Flexibilidad)
    public string? TipoRegistro { get; set; }
    public string? EjercicioNombre { get; set; }
    public decimal? ValorMetrica { get; set; }
    public string? UnidadMetrica { get; set; }

    public string? RutaFotoFrente { get; set; }
    public string? RutaFotoPerfil { get; set; }
    public string? RutaFotoEspalda { get; set; }

    public DateTime CreadoAt { get; set; } = DateTime.UtcNow;
}
