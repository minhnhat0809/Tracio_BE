using AutoMapper;
using NetTopologySuite.Geometries;
using RouteService.Application.DTOs.Routes;
using RouteService.Domain.Entities;
using Coordinate = RouteService.Application.DTOs.Routes.Coordinate;
using System.Text.Json;

namespace RouteService.Application.Mappings
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            // Route -> RouteMap4DViewModel
            CreateMap<Route, RouteMap4DViewModel>()
                .ForMember(dest => dest.Origin, opt => opt.MapFrom(src => ToCoordinate(src.StartLocation)))
                .ForMember(dest => dest.Destination, opt => opt.MapFrom(src => ToCoordinate(src.EndLocation)))
                .ForMember(dest => dest.Waypoints, opt => opt.MapFrom(src => ToCoordinates(src.Waypoints)))
                .ForMember(dest => dest.Avoid, opt => opt.MapFrom(src => ToCoordinate(src.Avoid)))
                .ForMember(dest => dest.AvoidsRoads, opt => opt.MapFrom(src => DeserializeJson<List<string>>(src.AvoidsRoads)));

            // Route -> RouteDetailViewModel (inherits from RouteMap4DViewModel)
            CreateMap<Route, RouteDetailViewModel>()
                .IncludeBase<Route, RouteMap4DViewModel>()
                .ForMember(dest => dest.CyclistId, opt => opt.MapFrom(src => src.CyclistId))
                .ForMember(dest => dest.CyclistName, opt => opt.MapFrom(src => src.CyclistName))
                .ForMember(dest => dest.CyclistAvatar, opt => opt.MapFrom(src => src.CyclistAvatar))
                .ForMember(dest => dest.TotalDistance, opt => opt.MapFrom(src => src.TotalDistance))
                .ForMember(dest => dest.ElevationGain, opt => opt.MapFrom(src => src.ElevationGain))
                .ForMember(dest => dest.MovingTime, opt => opt.MapFrom(src => src.MovingTime))
                .ForMember(dest => dest.AvgSpeed, opt => opt.MapFrom(src => src.AvgSpeed))
                .ForMember(dest => dest.Calories, opt => opt.MapFrom(src => src.Calories))
                .ForMember(dest => dest.Mood, opt => opt.MapFrom(src => src.Mood))
                .ForMember(dest => dest.ReactionCounts, opt => opt.MapFrom(src => src.ReactionCounts))
                .ForMember(dest => dest.Difficulty, opt => opt.MapFrom(src => src.Difficulty))
                .ForMember(dest => dest.IsPublic, opt => opt.MapFrom(src => src.IsPublic))
                .ForMember(dest => dest.IsGroup, opt => opt.MapFrom(src => src.IsGroup))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.TotalBookmarks, opt => opt.MapFrom(src => src.RouteBookmarks.Count))
                .ForMember(dest => dest.TotalReactions, opt => opt.MapFrom(src => src.RouteReactions.Count));
                
            // RouteRequest -> Route
            CreateMap<RouteRequest, Route>()
                .ForMember(dest => dest.StartLocation, opt => opt.MapFrom(src => ToPoint(src.Origin)))
                .ForMember(dest => dest.EndLocation, opt => opt.MapFrom(src => ToPoint(src.Destination)))
                .ForMember(dest => dest.RoutePath, opt => opt.MapFrom(src => ToLineString(src.Origin, src.Destination, src.Waypoints)))
                .ForMember(dest => dest.Waypoints, opt => opt.MapFrom(src => src.Waypoints.Any() ? ToLineString(src.Waypoints) : null))
                .ForMember(dest => dest.Avoid, opt => opt.MapFrom(src => src.Avoid != null ? ToPoint(src.Avoid) : null))
                .ForMember(dest => dest.AvoidsRoads, opt => opt.MapFrom(src => SerializeJson(src.AvoidsRoads)))
                .ForMember(dest => dest.OptimizeRoute, opt => opt.MapFrom(src => src.Optimize))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        }

        // Helper Methods for Mapping Spatial Types
        private static Coordinate? ToCoordinate(Point? point) =>
            point != null ? new Coordinate { Latitude = point.Y, Longitude = point.X } : null;

        private static List<Coordinate>? ToCoordinates(LineString? lineString) =>
            lineString?.Coordinates.Select(c => new Coordinate { Latitude = c.Y, Longitude = c.X }).ToList();

        private static Point ToPoint(Coordinate coord) => new Point(coord.Longitude, coord.Latitude) { SRID = 4326 };

        private static LineString ToLineString(Coordinate origin, Coordinate destination, List<Coordinate> waypoints) =>
            new LineString(waypoints.Select(wp => new NetTopologySuite.Geometries.Coordinate(wp.Longitude, wp.Latitude))
                .Prepend(new NetTopologySuite.Geometries.Coordinate(origin.Longitude, origin.Latitude))
                .Append(new NetTopologySuite.Geometries.Coordinate(destination.Longitude, destination.Latitude))
                .ToArray()) { SRID = 4326 };

        private static LineString ToLineString(List<Coordinate> waypoints) =>
            new LineString(waypoints.Select(wp => new NetTopologySuite.Geometries.Coordinate(wp.Longitude, wp.Latitude)).ToArray()) { SRID = 4326 };

        // JSON Handling Methods
        private static T? DeserializeJson<T>(string? json) =>
            !string.IsNullOrEmpty(json) ? JsonSerializer.Deserialize<T>(json) : default;

        private static string? SerializeJson<T>(T? obj) =>
            obj != null && obj is List<string> list && list.Any() ? JsonSerializer.Serialize(obj) : null;
    }
}
