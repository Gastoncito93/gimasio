namespace Backend.DTOs.Plan;

public class PlanSearchResultDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal PrecioMensual { get; set; }
}
