using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using NetTopologySuite.Geometries;
using RouteService.Application.DTOs.Coordinates;

namespace RouteService.Application.DTOs.RouteMediaFiles
{
    public class RouteMediaFileRequestModel
    {
        [Required]
        public required int CyclistId { get; set; }

        [Required]
        public required IFormFile MediaFile { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        public required CoordinateModel Location { get; set; } // Use NetTopologySuite Coordinate
    }
}