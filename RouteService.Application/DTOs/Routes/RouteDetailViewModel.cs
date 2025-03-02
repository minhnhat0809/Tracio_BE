using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using RouteService.Application.DTOs.RouteComment;
using RouteService.Application.DTOs.RouteMediaFiles;

namespace RouteService.Application.DTOs.Routes
{
    public class RouteDetailViewModel : RouteResponseModel
    {

        public string RouteName { get; set; } = null!;

        public string? Description { get; set; }

        public string? City { get; set; }

        public sbyte? Mood { get; set; }

        public int ReactionCounts { get; set; }

        public sbyte Difficulty { get; set; }

        public sbyte PrivacyLevel { get; set; }

        public bool? IsGroup { get; set; }

        public bool? IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}