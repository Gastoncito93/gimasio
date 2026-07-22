namespace Backend.Models;

public class Plan
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal PrecioMensual { get; set; }
    public string Estado { get; set; } = "Activo";

    public ICollection<Socio> Socios { get; set; } = new List<Socio>();
}
