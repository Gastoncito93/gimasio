namespace Backend.DTOs.Plan;

public class PlanResponseDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal PrecioMensual { get; set; }
    public string Estado { get; set; } = string.Empty;
}
