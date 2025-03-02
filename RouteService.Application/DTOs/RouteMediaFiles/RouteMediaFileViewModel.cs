using System;
using NetTopologySuite.Geometries;
using RouteService.Application.DTOs.Coordinates;

namespace RouteService.Application.DTOs.RouteMediaFiles
{
    public class RouteMediaFileViewModel
    {
        public int MediaId { get; set; }

        public int RouteId { get; set; }

        public int CyclistId { get; set; }

        public string MediaUrl { get; set; } = null!;

        public CoordinateModel Location { get; set; } = null!;

        public DateTime CapturedAt { get; set; }

        public DateTime UploadedAt { get; set; }
    }
}