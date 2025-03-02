using NetTopologySuite.Geometries;
using RouteService.Application.DTOs.Coordinates;

namespace RouteService.Application.DTOs.Routes;

public class RouteResponseModel
{
    public int RouteId { get; set; }

    public int CyclistId { get; set; }

    public CoordinateModel StartLocation { get; set; } = null!;

    public CoordinateModel EndLocation { get; set; } = null!;

    public List<CoordinateModel> RoutePath { get; set; } = null!;

    public List<CoordinateModel> Waypoints { get; set; } = null!;

    public string PolylineOverview { get; set; } = null!; 

    public List<string>? AvoidsRoads { get; set; }

    public bool OptimizeRoute { get; set; }

    public float TotalDistance { get; set; }

    public float ElevationGain { get; set; }

    public int MovingTime { get; set; }

    public int DurationTime { get; set; }

    public int StoppedTime { get; set; }

    public float AvgSpeed { get; set; }

    public float MaxSpeed { get; set; }

    public decimal Calories { get; set; }
    
    public DateTime CreatedAt { get; set; }
}