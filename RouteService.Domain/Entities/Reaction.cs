using System;
using System.Collections.Generic;
namespace RouteService.Domain.Entities;

public partial class Reaction
{
    public int ReactionId { get; set; }

    public int CyclistId { get; set; }

    public int TargetId { get; set; }

    public string TargetType { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
