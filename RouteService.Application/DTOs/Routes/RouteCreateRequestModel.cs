
using System.ComponentModel.DataAnnotations;
using NetTopologySuite.Geometries;
using RouteService.Application.DTOs.Coordinates;

namespace RouteService.Application.DTOs.Routes
{
    public class RouteCreateRequestModel
    {
        [Required] 
        public required int CyclistId { get; set; }
        public string? RouteName { get; set; } // set Untitled Route
        
        // Place information
        public string? City { get; set; }

        // Map information
        [Required(ErrorMessage = "Origin is required.")]
        public required CoordinateModel Origin { get; set; } // start point
        
        [Required(ErrorMessage = "Destination is required.")]
        public required CoordinateModel Destination { get; set; } // end point
        
        public List<CoordinateModel> Waypoints { get; set; } = new(); // List of waypoints
        
        public string PolylineOverview { get; set; } = null!;

        public List<string>? AvoidsRoads { get; set; } 

        public bool OptimizeRoute { get; set; } = false;

        public bool IsValid(out List<string> validationErrors)
        {
            validationErrors = new List<string>();

            if (Waypoints.Count > 25)
                validationErrors.Add("Waypoints cannot exceed 25 points.");

            if (AvoidsRoads != null && AvoidsRoads.Any(r => !new[] { "motorway", "trunk", "ferry", "bridge", "tunnel", "toll" }.Contains(r)))
                validationErrors.Add("Invalid road type in AvoidsRoads.");

            return !validationErrors.Any();
        }
    }
}

