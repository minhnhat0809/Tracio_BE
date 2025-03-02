﻿using System;
using System.Collections.Generic;
namespace RouteService.Domain.Entities;

public partial class RouteBookmark
{
    public int BookmarkId { get; set; }

    public int CyclistId { get; set; }

    public int RouteId { get; set; }

    public DateTime CreatedAt { get; set; }
}
