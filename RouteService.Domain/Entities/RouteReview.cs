using System;
using System.Collections.Generic;

namespace RouteService.Domain.Entities;

public partial class RouteReview
{
    public int ReviewId { get; set; }

    public int RouteId { get; set; }

    public int CyclistId { get; set; }

    public string CyclistName { get; set; } = null!;

    public string CyclistAvatar { get; set; } = null!;

    public sbyte? Rating { get; set; }

    public string? ReviewContent { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Route Route { get; set; } = null!;
}
