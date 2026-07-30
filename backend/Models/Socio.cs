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
    public string? Observacion { get; set; }

    public int? IdPlan { get; set; }
    public Plan? Plan { get; set; }

    public int? IdUsuario { get; set; }
    public Usuario? Usuario { get; set; }

    public int? IdCoach { get; set; }
    public Usuario? Coach { get; set; }

    public ICollection<Cuota> Cuotas { get; set; } = new List<Cuota>();
    public ICollection<Certificado> Certificados { get; set; } = new List<Certificado>();
    public ICollection<SocioProgreso> Progresos { get; set; } = new List<SocioProgreso>();
}
