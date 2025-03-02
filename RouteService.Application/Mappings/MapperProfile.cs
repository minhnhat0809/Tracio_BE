using AutoMapper;
using RouteService.Application.DTOs.Routes;
using RouteService.Domain.Entities;
using System.Text.Json;
using NetTopologySuite.Geometries;
using RouteService.Application.DTOs.Bookmarks;
using RouteService.Application.DTOs.Coordinates;
using RouteService.Application.DTOs.Reacts;
using RouteService.Application.DTOs.RouteComment;
using RouteService.Application.DTOs.RouteMediaFiles;
using RouteService.Application.Interfaces.Services;

namespace RouteService.Application.Mappings
{
     public class MapperProfile : Profile
        {
            public MapperProfile()
            {
                // ROUTE -> RouteResponseModel
                CreateMap<Route, RouteResponseModel>()
                    .ForMember(dest => dest.StartLocation, opt => opt.MapFrom(src => ConvertToCoordinateModel(src.StartLocation)))
                    .ForMember(dest => dest.EndLocation, opt => opt.MapFrom(src => ConvertToCoordinateModel(src.EndLocation)))
                    .ForMember(dest => dest.RoutePath, opt => opt.MapFrom(src => ConvertToCoordinateList(src.RoutePath)))
                    .ForMember(dest => dest.Waypoints, opt => opt.MapFrom(src => ConvertToCoordinateList(src.Waypoints)))
                    .ForMember(dest => dest.AvoidsRoads, opt => opt.MapFrom(src => 
                        string.IsNullOrEmpty(src.AvoidsRoads) ? null : DeserializeJson<List<string>>(src.AvoidsRoads)
                    ));

                // Route -> RouteDetailViewModel (inherits from RouteResponseModel)
                CreateMap<Route, RouteDetailViewModel>()
                    .IncludeBase<Route, RouteResponseModel>() // Inherit mappings from RouteResponseModel
                    .ForMember(dest => dest.RouteName, opt => opt.MapFrom(src => src.RouteName))
                    .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                    .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                    .ForMember(dest => dest.Mood, opt => opt.MapFrom(src => src.Mood))
                    .ForMember(dest => dest.ReactionCounts, opt => opt.MapFrom(src => src.ReactionCounts))
                    .ForMember(dest => dest.Difficulty, opt => opt.MapFrom(src => src.Difficulty))
                    .ForMember(dest => dest.PrivacyLevel, opt => opt.MapFrom(src => src.PrivacyLevel))
                    .ForMember(dest => dest.IsGroup, opt => opt.MapFrom(src => src.IsGroup))
                    .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
                    .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                    .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

                
                // RouteCreateRequestModel -> Route
                CreateMap<RouteCreateRequestModel, Route>()
                    .ForMember(dest => dest.StartLocation, opt => opt.MapFrom(src => ConvertToPoint(src.Origin)))
                    .ForMember(dest => dest.EndLocation, opt => opt.MapFrom(src => ConvertToPoint(src.Destination)))
                    .ForMember(dest => dest.Waypoints, opt => opt.MapFrom(src => ConvertToLineString(src.Waypoints)))
                    .ForMember(dest => dest.AvoidsRoads, opt => opt.MapFrom(src => SerializeJson(src.AvoidsRoads)))
                    .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

                // RouteUpdateRequestModel -> Route
                CreateMap<RouteUpdateRequestModel, Route>()
                    .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
                
                // REACT
                CreateMap<Reaction, ReactViewModel>();
                
                
                // COMMENT
                CreateMap<RouteComment, CommentViewModel>();
                CreateMap<RouteComment, ReplyViewModel>();
                CreateMap<RouteUpdateRequestModel, Route>()
                    .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
                
                
                
                // Media File
                CreateMap<RouteMediaFile, RouteMediaFileViewModel>()
                    .ForMember(dest => dest.Location, opt => opt.MapFrom(src => 
                            src.Location != null && IsValidCoordinate(src.Location)
                                ? new CoordinateModel
                                {
                                    Latitude = src.Location.Y,
                                    Longitude = src.Location.X,
                                    Altitude = double.IsFinite(src.Location.Z) ? src.Location.Z : 0.0 // Ensure Z is finite
                                }
                                : new CoordinateModel { Latitude = 0, Longitude = 0, Altitude = 0 } // Fallback for invalid points
                    ));
                   
                CreateMap<RouteMediaFileRequestModel, RouteMediaFile>() 
                    .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

                // BOOKMARK ROUTE
                CreateMap<RouteBookmark, BookmarkViewModel>();
            }
            
           // Convert Point to CoordinateModel
        private static CoordinateModel ConvertToCoordinateModel(Point point)
        {
            return point != null && IsValidCoordinate(point)
                ? new CoordinateModel
                {
                    Latitude = point.Y,
                    Longitude = point.X,
                    Altitude = double.IsFinite(point.Z) ? point.Z : 0.0
                }
                : new CoordinateModel { Latitude = 0, Longitude = 0, Altitude = 0 };
        }

        // Convert LineString to List<CoordinateModel>
        private static List<CoordinateModel> ConvertToCoordinateList(LineString lineString)
        {
            return lineString?.Coordinates.Select(coord => new CoordinateModel
            {
                Latitude = coord.Y,
                Longitude = coord.X,
                Altitude = double.IsFinite(coord.Z) ? coord.Z : 0.0
            }).ToList() ?? new List<CoordinateModel>();
        }

        // Convert CoordinateModel to Point
        private static Point ConvertToPoint(CoordinateModel model)
        {
            return new Point(model.Longitude, model.Latitude, model.Altitude) { SRID = 4326 };
        }

        // Convert List<CoordinateModel> to LineString
        private static LineString ConvertToLineString(List<CoordinateModel> coordinates)
        {
            if (coordinates == null || coordinates.Count == 0) return null;

            var coordArray = coordinates
                .Select(coord => new CoordinateZ(coord.Longitude, coord.Latitude, coord.Altitude))
                .ToArray();
    
            return new LineString(coordArray) { SRID = 4326 };
        }

        private static bool IsValidCoordinate(Point point)
        {
            return double.IsFinite(point.X) && double.IsFinite(point.Y);
        }

        private static T? DeserializeJson<T>(string? json) =>
            !string.IsNullOrEmpty(json) ? JsonSerializer.Deserialize<T>(json) : default;

        private static string? SerializeJson<T>(T? obj) =>
            obj != null && obj is List<string> list && list.Any() ? JsonSerializer.Serialize(obj) : null;
        }


}
