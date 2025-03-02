using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
namespace RouteService.Domain.Entities;

public partial class Route
{
    public int RouteId { get; set; }

    public int CyclistId { get; set; }

    public string RouteName { get; set; } = null!;

    public string? Description { get; set; }

    public string? City { get; set; }

    public Point StartLocation { get; set; } = null!;

    public Point EndLocation { get; set; } = null!;

    public LineString RoutePath { get; set; } = null!;

    public LineString Waypoints { get; set; } = null!;

    public string PolylineOverview { get; set; } = null!;

    public string? AvoidsRoads { get; set; }

    public bool OptimizeRoute { get; set; }

    public float TotalDistance { get; set; }

    public float ElevationGain { get; set; }

    public int MovingTime { get; set; }

    public int DurationTime { get; set; }

    public int StoppedTime { get; set; }

    public float AvgSpeed { get; set; }

    public float MaxSpeed { get; set; }

    public decimal Calories { get; set; }

    public sbyte? Mood { get; set; }

    public int ReactionCounts { get; set; }

    public int CommentCounts { get; set; }

    public sbyte Difficulty { get; set; }

    public sbyte PrivacyLevel { get; set; }

    public bool? IsGroup { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<RouteComment> RouteComments { get; set; } = new List<RouteComment>();

    public virtual ICollection<RouteMediaFile> RouteMediaFiles { get; set; } = new List<RouteMediaFile>();
}
