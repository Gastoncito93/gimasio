namespace Backend.DTOs.Socio;

public class SocioSearchResultDto
{
    public int Id { get; set; }
    public string Dni { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string PlanNombre { get; set; } = string.Empty;
}
