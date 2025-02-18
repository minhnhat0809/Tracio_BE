
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace RouteService.Application.DTOs.Routes
{
    public class RouteRequest
    {
        [Required(ErrorMessage = "Origin is required.")]
        public required Coordinate Origin { get; set; }

        [Required(ErrorMessage = "Destination is required.")]
        public required Coordinate Destination { get; set; }

        public List<Coordinate> Waypoints { get; set; } = new();

        [Required(ErrorMessage = "TravelMode is required.")]
        [RegularExpression("^(car|bike|foot|motorcycle)$", ErrorMessage = "Invalid travel mode. Allowed values: car, bike, foot, motorcycle.")]
        public string TravelMode { get; set; } = "bike";

        [Range(0, 2, ErrorMessage = "Weighting must be 0 (shortest), 1 (fastest), or 2 (balanced).")]
        public int Weighting { get; set; } = 0;

        public Coordinate? Avoid { get; set; }

        public List<string>? AvoidsRoads { get; set; }

        public bool Optimize { get; set; } = false;

        public bool IsValid(out List<string> validationErrors)
        {
            validationErrors = new List<string>();

            if (Origin == null || !Origin.IsValid())
                validationErrors.Add("Invalid Origin coordinates.");

            if (Destination == null || !Destination.IsValid())
                validationErrors.Add("Invalid Destination coordinates.");

            if (Waypoints.Count > 25)
                validationErrors.Add("Waypoints cannot exceed 25 points.");

            if (AvoidsRoads != null && AvoidsRoads.Any(r => !new[] { "motorway", "trunk", "ferry", "bridge", "tunnel", "toll" }.Contains(r)))
                validationErrors.Add("Invalid road type in AvoidsRoads.");

            return !validationErrors.Any();
        }
    }

    public class Coordinate
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public bool IsValid()
        {
            return Latitude is >= -90 and <= 90 && Longitude is >= -180 and <= 180;
        }
    }
}
