using RouteService.Application.DTOs.Points;

namespace RouteService.Application.DTOs.Routes;

public class RouteRequestModel
{
    public string RouteName { get; set; } = null!;
    public int CyclistId { get; set; }
    public List<PointRequestModel> RoutePath { get; set; } = new List<PointRequestModel>();
}