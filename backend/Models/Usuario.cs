namespace Backend.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? RutaAvatar { get; set; }
    public bool DebeCambiarPassword { get; set; } = true;
    public string? TokenRecuperacion { get; set; }
    public DateTime? TokenRecuperacionExpiracion { get; set; }
    public DateTime? EliminadoAt { get; set; }

    public int IdRol { get; set; }
    public Rol Rol { get; set; } = null!;

    public int? IdActividad { get; set; }
    public Actividad? Actividad { get; set; }

    public ICollection<Socio> AlumnosComoCoach { get; set; } = new List<Socio>();
}
