using System;
using System.Collections.Generic;

namespace RouteService.Application.DTOs.Routes
{
    public class RouteMap4DViewModel
    {
        public int RouteId { get; set; }
        public Coordinate Origin { get; set; } = null!;
        public Coordinate Destination { get; set; } = null!;
        public List<Coordinate>? Waypoints { get; set; } // If no waypoints, it should be null.

        // Additional Route Parameters
        public int Weighting { get; set; } // 0: Shortest, 1: Fastest, 2: Balanced
        public Coordinate? Avoid { get; set; } // Coordinate point to avoid
        public List<string>? AvoidsRoads { get; set; } // List of road types to avoid
        public bool OptimizeRoute { get; set; } // Optimize route ordering
    }
}