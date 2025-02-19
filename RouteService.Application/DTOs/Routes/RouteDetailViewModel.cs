using System;
using System.Collections.Generic;
using RouteService.Application.DTOs.RouteMediaFiles;
using RouteService.Application.DTOs.RouteReviews;

namespace RouteService.Application.DTOs.Routes
{
    public class RouteDetailViewModel : RouteMap4DViewModel
    {
        public int RouteId { get; set; }
        public int CyclistId { get; set; }
        public string CyclistName { get; set; } = null!;
        public string CyclistAvatar { get; set; } = null!;
        public string RouteName { get; set; } = null!;
        
        public Coordinate Origin { get; set; } = null!;
        public Coordinate Destination { get; set; } = null!;
        //public List<Coordinate>? Waypoints { get; set; } // If no waypoints, it should be null.

        public int Weighting { get; set; }
        public Coordinate? Avoid { get; set; }
        public List<string>? AvoidsRoads { get; set; }
        public bool OptimizeRoute { get; set; }

        public float TotalDistance { get; set; }
        public float ElevationGain { get; set; }
        public float MovingTime { get; set; }
        public float AvgSpeed { get; set; }
        public float Calories { get; set; }

        public sbyte? Mood { get; set; }
        public int? ReactionCounts { get; set; }
        public sbyte Difficulty { get; set; }
        public bool? IsPublic { get; set; }
        public bool? IsGroup { get; set; }
        public bool? IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }

        // Additional data from related collections
        public List<RouteMediaFileViewModel>? MediaFiles { get; set; }
        public List<RouteReviewViewModel>? Reviews { get; set; }
        public int TotalBookmarks { get; set; }
        public int TotalReactions { get; set; }
    }
}