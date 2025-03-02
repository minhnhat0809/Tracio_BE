using System.Text.Json;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using RouteService.Application.DTOs;
using RouteService.Application.DTOs.Routes;
using RouteService.Domain.Entities;
using Coordinate = NetTopologySuite.Geometries.Coordinate;

namespace RouteService.Application.Interfaces.Services;

public interface IRouteService
{
    // Routing
    Task<ResponseModel> GetAllRoutesAsync(
        int pageNumber, int rowsPerPage,
        Dictionary<string, string>? filters = null,
        string? sortField = null, bool sortDesc = false);
    Task<ResponseModel?> GetRouteByIdAsync(int routeId);
    Task<ResponseModel> CreateRouteAsync(RouteCreateRequestModel request);
    Task<ResponseModel> UpdateRouteAsync(int routeId, RouteUpdateRequestModel request);
    Task<ResponseModel> SoftDeleteRouteAsync(int routeId);
    
    // Tracking
    Task<ResponseModel> TrackingInRouteAsync(int routeId, TrackingRequestModel request);
    Task<ResponseModel> StartTrackingAsync(int routeId);
    Task<ResponseModel> FinishTrackingAsync(int routeId);
    
}
public class RouteService : IRouteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<RouteService> _logger;

    public RouteService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<RouteService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ResponseModel> GetAllRoutesAsync(
        int pageNumber, int rowsPerPage,
        Dictionary<string, string>? filters = null,
        string? sortField = null, bool sortDesc = false)
    {
        var (routes, totalCount) = await _unitOfWork.RouteRepository.GetAllAsync(
            pageNumber, rowsPerPage, sortField, sortDesc, filters);

        if (!routes.Any())
            return new ResponseModel(null, "No routes found.", false, 404);

        var routeViewModels = _mapper.Map<List<RouteDetailViewModel>>(routes);
        return new ResponseModel((routeViewModels, totalCount), "Routes retrieved successfully.", true, 200);
    }

    public async Task<ResponseModel?> GetRouteByIdAsync(int routeId)
    {
        var route = await _unitOfWork.RouteRepository.GetByIdAsync(routeId);
        if (route == null)
            return new ResponseModel(null, $"Route with ID {routeId} not found.", false, 404);

        var routeViewModel = _mapper.Map<RouteDetailViewModel>(route);
        return new ResponseModel(routeViewModel, "Route retrieved successfully.", true, 200);
    }
    
    
    /// <summary>
    /// Create route base on Location Input. Create Before go Tracking
    /// </summary>
    public async Task<ResponseModel> CreateRouteAsync(RouteCreateRequestModel request)
    {
        try
        {
            if (!request.IsValid(out var errors))
                return new ResponseModel(null, $"Validation failed: {string.Join(", ", errors)}", false, 400);
        
            var newRoute = _mapper.Map<Route>(request);

            // Convert Start and End locations
            var startLocation = new CoordinateZ(request.Origin.Longitude, request.Origin.Latitude, request.Origin.Altitude);
            var endLocation = new CoordinateZ(request.Destination.Longitude, request.Destination.Latitude, request.Destination.Altitude);
            
            // Construct RoutePath: [StartLocation, Waypoints, EndLocation]
            var routeCoordinates = new List<CoordinateZ> { startLocation };
            if (request.Waypoints.Any())
            {
                routeCoordinates.AddRange(request.Waypoints.Select(coord => new CoordinateZ(coord.Longitude, coord.Latitude, coord.Altitude)));
            }
            routeCoordinates.Add(endLocation);

            // Convert to LineString
            var routePath = new LineString(routeCoordinates.ToArray()) { SRID = 4326 };
            newRoute.RoutePath = routePath;
            
            // Serialize AvoidsRoads
            newRoute.AvoidsRoads = request.AvoidsRoads != null && request.AvoidsRoads.Any()
                ? JsonSerializer.Serialize(request.AvoidsRoads)
                : null;

            var createdRoute = await _unitOfWork.RouteRepository.CreateAsync(newRoute);

            // Explicitly deserialize AvoidsRoads before returning response
            var responseModel = _mapper.Map<RouteResponseModel>(createdRoute);
            return new ResponseModel(responseModel, "Route created successfully.", true, 201);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating route");
            return new ResponseModel(null, "An error occurred while creating the route.", false, 500);
        }
    }
    
    /// <summary>
    /// Update route basic information. Update After go Tracking Finished or Public it
    /// </summary>
    public async Task<ResponseModel> UpdateRouteAsync(int routeId, RouteUpdateRequestModel request)
    {
        var existingRoute = await _unitOfWork.RouteRepository.GetByIdAsync(routeId);
        if (existingRoute == null)
            return new ResponseModel(null, $"Route with ID {routeId} not found.", false, 404);

        _mapper.Map(request, existingRoute);
        
        existingRoute.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.RouteRepository.UpdateAsync(existingRoute);

        return new ResponseModel(_mapper.Map<RouteDetailViewModel>(existingRoute), "Route updated successfully.", true, 200);
    }

    /// <summary>
    /// Delete route.
    /// </summary>
    public async Task<ResponseModel> SoftDeleteRouteAsync(int routeId)
    {
        var existingRoute = await _unitOfWork.RouteRepository.GetByIdAsync(routeId);
        if (existingRoute == null)
            return new ResponseModel(null, $"Route with ID {routeId} not found.", false, 404);

        existingRoute.IsDeleted = true;
        existingRoute.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.RouteRepository.UpdateAsync(existingRoute);

        return new ResponseModel(null, "Route soft deleted successfully.", true, 200);
    }

    /// <summary>
    /// Send User Location to check if on Route Track and Update to Redis 
    /// </summary>
    public Task<ResponseModel> TrackingInRouteAsync(int routeId, TrackingRequestModel request)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Start to Tracking 
    /// </summary>
    public Task<ResponseModel> StartTrackingAsync(int routeId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Finish Tracking. Receive user basic data while do tracking and update to Route, ready for public 
    /// </summary>
    public Task<ResponseModel> FinishTrackingAsync(int routeId)
    {
        throw new NotImplementedException();
    }
}



