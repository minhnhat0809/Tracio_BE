using System;
using System.Collections.Generic;

namespace RouteService.Domain.Entities;

public partial class RouteReaction
{
    public int ReactionId { get; set; }

    public int RouteId { get; set; }

    public int CyclistId { get; set; }

    public string ReactionType { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Route Route { get; set; } = null!;
}
