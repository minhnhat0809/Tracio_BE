using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using NetTopologySuite.Geometries;
using RouteService.Application.DTOs;
using RouteService.Application.DTOs.Routes;
using RouteService.Domain.Entities;
using Coordinate = NetTopologySuite.Geometries.Coordinate;

namespace RouteService.Application.Interfaces.Services;

public interface IRouteService
{
    Task<ResponseModel> GetAllRoutesAsync(int pageIndex, int pageSize);
    Task<ResponseModel?> GetRouteMap4DByIdAsync(int id);
    Task<ResponseModel?> GetRouteDetailByIdAsync(int id);
    Task<ResponseModel> CreateRouteAsync(RouteRequest request);
}

public class RouteService : IRouteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RouteService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all routes with pagination
    /// </summary>
    public async Task<ResponseModel> GetAllRoutesAsync(int pageIndex, int pageSize)
    {
        var routes = await _unitOfWork.RouteRepository.GetAllAsync(null, pageIndex, pageSize, "CreatedAt", sortDesc: true);

        if (!routes.Any())
            return new ResponseModel(null, "No routes found.", false, 404);

        var routeViewModels = _mapper.Map<List<RouteDetailViewModel>>(routes);
        return new ResponseModel(routeViewModels, "Routes retrieved successfully.", true, 200);
    }

    /// <summary>
    /// Get a specific route by ID
    /// </summary>
    public async Task<ResponseModel?> GetRouteMap4DByIdAsync(int id)
    {
        var route = await _unitOfWork.RouteRepository.GetByIdAsync(id);

        if (route == null)
            return new ResponseModel(null, $"Route with ID {id} not found.", false, 404);

        var routeViewModel = _mapper.Map<RouteMap4DViewModel>(route);
        return new ResponseModel(routeViewModel, "Route retrieved successfully.", true, 200);
    }
    /// <summary>
    /// Get a specific route by ID
    /// </summary>
    public async Task<ResponseModel?> GetRouteDetailByIdAsync(int id)
    {
        var route = await _unitOfWork.RouteRepository.GetByIdAsync(id);

        if (route == null)
            return new ResponseModel(null, $"Route with ID {id} not found.", false, 404);

        var routeViewModel = _mapper.Map<RouteDetailViewModel>(route);
        return new ResponseModel(routeViewModel, "Route retrieved successfully.", true, 200);
    }

    /// <summary>
    /// Create a new route after converting coordinates to spatial types
    /// </summary>
    public async Task<ResponseModel> CreateRouteAsync(RouteRequest request)
{
    try
    {
        // Validate request manually
        if (!request.IsValid(out var errors))
        {
            return new ResponseModel(null, $"Validation failed: {string.Join("; ", errors)}", false, 400);
        }

        // Convert Origin and Destination to POINT
        var startLocation = new Point(request.Origin.Longitude, request.Origin.Latitude) { SRID = 4326 };
        var endLocation = new Point(request.Destination.Longitude, request.Destination.Latitude) { SRID = 4326 };

        // Convert RoutePath (Full Path including Origin, Destination, and Waypoints)
        var routeCoordinates = new List<Coordinate> { new(request.Origin.Longitude, request.Origin.Latitude) };
        routeCoordinates.AddRange(request.Waypoints.Select(wp => new Coordinate(wp.Longitude, wp.Latitude)));
        routeCoordinates.Add(new Coordinate(request.Destination.Longitude, request.Destination.Latitude));
        var routePath = new LineString(routeCoordinates.ToArray()) { SRID = 4326 };

        // Convert Waypoints separately (Only if waypoints exist)
        LineString? waypoints = request.Waypoints.Any()
            ? new LineString(request.Waypoints.Select(wp => new Coordinate(wp.Longitude, wp.Latitude)).ToArray()) { SRID = 4326 }
            : null;

        // Convert Avoid to POINT if exists
        Point? avoidPoint = request.Avoid != null
            ? new Point(request.Avoid.Longitude, request.Avoid.Latitude) { SRID = 4326 }
            : null;

        // Convert AvoidsRoads to JSON string (if empty, store NULL)
        string? avoidsRoadsJson = request.AvoidsRoads != null && request.AvoidsRoads.Any()
            ? System.Text.Json.JsonSerializer.Serialize(request.AvoidsRoads)
            : null;

        // Create new Route entity with all fields mapped
        var newRoute = new Route
        {
            CyclistId = 1, // Temporary value, should be fetched from auth context
            CyclistName = "Sample Cyclist",
            CyclistAvatar = "https://example.com/avatar.jpg",
            RouteName = $"Route from {request.Origin.Latitude},{request.Origin.Longitude} to {request.Destination.Latitude},{request.Destination.Longitude}",
            StartLocation = startLocation,
            EndLocation = endLocation,
            RoutePath = routePath,
            Waypoints = waypoints, // Separate Waypoints

            // Navigation & Avoidance
            Weighting = (sbyte)request.Weighting,
            Avoid = avoidPoint,
            AvoidsRoads = avoidsRoadsJson,
            OptimizeRoute = request.Optimize,

            // Additional data
            TotalDistance = 0, // Placeholder, should be calculated
            ElevationGain = 0, // Placeholder
            MovingTime = 0, // Placeholder
            AvgSpeed = 0, // Placeholder
            Calories = 0, // Placeholder

            // User Engagement Data
            Mood = 0,
            ReactionCounts = 0,
            Difficulty = 0,
            IsPublic = false,
            IsGroup = false,
            IsDeleted = false,

            CreatedAt = DateTime.UtcNow
        };

        var createdRoute = await _unitOfWork.RouteRepository.CreateAsync(newRoute);

        // Return response with correct mapping
        return new ResponseModel(_mapper.Map<RouteMap4DViewModel>(createdRoute), "Route created successfully.", true, 201);
    }
    catch (Exception ex)
    {
        return new ResponseModel(null, $"Error: {ex.Message}", false, 500);
    }
}



}

