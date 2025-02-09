using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace RouteService.Domain.Entities;

public partial class Route
{
    public int RouteId { get; set; }

    public int CyclistId { get; set; }

    public string RouteName { get; set; } = null!;

    public LineString RoutePath { get; set; } = null!;

    public Point StartLocation { get; set; } = null!;

    public float TotalDistance { get; set; }

    public float? Pace { get; set; }

    public float? TotalTime { get; set; }

    public sbyte? Mood { get; set; }

    public int? ReactionCounts { get; set; }

    public sbyte Difficulty { get; set; }

    public bool? IsPublic { get; set; }

    public bool? IsGroup { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<RouteBookmark> RouteBookmarks { get; set; } = new List<RouteBookmark>();

    public virtual ICollection<RouteMediaFile> RouteMediaFiles { get; set; } = new List<RouteMediaFile>();

    public virtual ICollection<RouteReaction> RouteReactions { get; set; } = new List<RouteReaction>();

    public virtual ICollection<RouteReview> RouteReviews { get; set; } = new List<RouteReview>();
}
