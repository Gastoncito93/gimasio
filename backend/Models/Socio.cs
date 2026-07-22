namespace Backend.Models;

public class Socio
{
    public int Id { get; set; }
    public string Dni { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public DateTime FechaAlta { get; set; } = DateTime.UtcNow;
    public string Estado { get; set; } = "Activo";

    public int IdPlan { get; set; }
    public Plan Plan { get; set; } = null!;

    public ICollection<Cuota> Cuotas { get; set; } = new List<Cuota>();
    public ICollection<Certificado> Certificados { get; set; } = new List<Certificado>();
}
