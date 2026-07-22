namespace Backend.Models;

public class Certificado
{
    public int Id { get; set; }
    public int IdSocio { get; set; }
    public Socio Socio { get; set; } = null!;

    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public string Estado { get; set; } = "Vigente";
}
