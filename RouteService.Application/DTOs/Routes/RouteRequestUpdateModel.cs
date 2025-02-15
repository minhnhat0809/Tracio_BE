using RouteService.Application.DTOs.Points;

namespace RouteService.Application.DTOs.Routes;

public class RouteRequestUpdateModel
{
    public int RouteId { get; set; }
    public PointRequestModel CurrentLocation { get; set; } = null!;
}