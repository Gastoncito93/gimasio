namespace Backend.Models;

public class Actividad
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string Estado { get; set; } = "Activo";
    public DateTime CreadoAt { get; set; } = DateTime.UtcNow;

    public ICollection<Usuario> Coaches { get; set; } = new List<Usuario>();
}
