namespace Backend.Models;

public class Cuota
{
    public int Id { get; set; }
    public int IdSocio { get; set; }
    public Socio Socio { get; set; } = null!;

    public int Periodo { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public DateTime? FechaPago { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public string? Observacion { get; set; }
}
