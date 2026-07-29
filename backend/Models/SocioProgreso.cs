namespace Backend.Models;

public class SocioProgreso
{
    public int Id { get; set; }

    public int IdSocio { get; set; }
    public Socio Socio { get; set; } = null!;

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    public decimal? PesoKg { get; set; }
    public string? Observaciones { get; set; }

    public string? RutaFotoFrente { get; set; }
    public string? RutaFotoPerfil { get; set; }
    public string? RutaFotoEspalda { get; set; }

    public DateTime CreadoAt { get; set; } = DateTime.UtcNow;
}
