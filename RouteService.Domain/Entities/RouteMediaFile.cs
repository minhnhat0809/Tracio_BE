using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
namespace RouteService.Domain.Entities;

public partial class RouteMediaFile
{
    public int MediaId { get; set; }

    public int RouteId { get; set; }

    public int CyclistId { get; set; }

    public string MediaUrl { get; set; } = null!;

    public Point Location { get; set; } = null!;

    public DateTime CapturedAt { get; set; }

    public DateTime UploadedAt { get; set; }

    public virtual Route Route { get; set; } = null!;
}
