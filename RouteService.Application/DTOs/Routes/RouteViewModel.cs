namespace RouteService.Application.DTOs.Routes;

public class RouteViewModel
{
    public int RouteId { get; set; }
    public string RouteName { get; set; } = null!;
    public float TotalDistance { get; set; }
    public float? Pace { get; set; }
    public float? TotalTime { get; set; }
}