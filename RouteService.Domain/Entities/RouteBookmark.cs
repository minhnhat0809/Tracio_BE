using System;
using System.Collections.Generic;

namespace RouteService.Domain.Entities;

public partial class RouteBookmark
{
    public int BookmarkId { get; set; }

    public int OwnerId { get; set; }

    public int RouteId { get; set; }

    public string CollectionName { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual Route Route { get; set; } = null!;
}
