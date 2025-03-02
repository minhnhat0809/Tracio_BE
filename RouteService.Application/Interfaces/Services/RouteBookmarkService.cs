using AutoMapper;
using RouteService.Application.DTOs;
using RouteService.Application.DTOs.Bookmarks;
using RouteService.Application.DTOs.Routes;
using RouteService.Domain.Entities;

namespace RouteService.Application.Interfaces.Services;
public interface IRouteBookmarkService
{
    // get routes bookmark by user id
    Task<ResponseModel> GetAllRouteBookmarksAsync(
        int userId,
        int pageNumber = 1, 
        int rowsPerPage = 10, 
        string? filterField = null, 
        string? filterValue = null,
        string? sortField = null, 
        bool sortDesc = false);
    
    Task<ResponseModel> CreateRouteBookmarkAsync(int userId, int routeId);
    Task<ResponseModel> DeleteRouteBookmarkAsync(int userId, int routeId);
}
public class RouteBookmarkService : IRouteBookmarkService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public RouteBookmarkService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all route bookmarks for a user with pagination & filtering
    /// </summary>
    public async Task<ResponseModel> GetAllRouteBookmarksAsync(
        int userId, int pageNumber = 1, int rowsPerPage = 10, 
        string? filterField = null, string? filterValue = null, 
        string? sortField = null, bool sortDesc = false)
    {
        try
        {
            /*var filters = new Dictionary<string, string> { { "UserId", userId.ToString() } };

            if (!string.IsNullOrEmpty(filterField) && !string.IsNullOrEmpty(filterValue))
            {
                filters.Add(filterField, filterValue);
            }

            var (routes, totalCount) = await _unitOfWork.RouteRepository.GetAllAsync(
                pageNumber, rowsPerPage, sortField, sortDesc, filters, "Bookmarks");*/
            var filters = new Dictionary<string, string>
                { { "CyclistId", userId.ToString() } };
            if (!string.IsNullOrEmpty(filterField) && !string.IsNullOrEmpty(filterValue))
            {
                filters.Add(filterField, filterValue);
            }
            var (routes, totalCount) = await _unitOfWork.RouteBookmarkRepository.GetAllAsync(
                pageNumber, rowsPerPage, sortField, sortDesc, filters);
            var routeBookmarks = routes.ToList();
            
            if (!routeBookmarks.Any())
                return new ResponseModel(null, "No bookmarks found.", false, 404);

            return new ResponseModel(_mapper.Map<List<BookmarkViewModel>>(routeBookmarks), "Bookmarks retrieved successfully.", true, 200);
        }
        catch (Exception ex)
        {
            return new ResponseModel(null, $"Error retrieving bookmarks: {ex.Message}", false, 500);
        }
    }

    /// <summary>
    /// Add a new bookmark (avoid duplicates)
    /// </summary>
    public async Task<ResponseModel> CreateRouteBookmarkAsync(int userId, int routeId)
    {
        try
        {
            var existingBookmark = await _unitOfWork.RouteBookmarkRepository.GetByFilterAsync(
                new Dictionary<string, string> { { "CyclistId", userId.ToString() }, { "RouteId", routeId.ToString() } });

            if (existingBookmark != null)
                return new ResponseModel(null, "Bookmark already exists.", false, 409);

            var newBookmark = new RouteBookmark
            {
                CyclistId = userId,
                RouteId = routeId,
                CreatedAt = DateTime.UtcNow
            };

            var createdBookmark = await _unitOfWork.RouteBookmarkRepository.CreateAsync(newBookmark);
            return new ResponseModel(_mapper.Map<BookmarkViewModel>(createdBookmark), "Bookmark created successfully.", true, 201);
        }
        catch (Exception ex)
        {
            return new ResponseModel(null, $"Error creating bookmark: {ex.Message}", false, 500);
        }
    }

    /// <summary>
    /// Remove a bookmark (if it exists)
    /// </summary>
    public async Task<ResponseModel> DeleteRouteBookmarkAsync(int userId, int routeId)
    {
        try
        {
            var existingBookmark = await _unitOfWork.RouteBookmarkRepository.GetByFilterAsync(
                new Dictionary<string, string> { { "CyclistId", userId.ToString() }, { "RouteId", routeId.ToString() } });

            if (existingBookmark == null)
                return new ResponseModel(null, "Bookmark not found.", false, 404);

            await _unitOfWork.RouteBookmarkRepository.DeleteAsync(existingBookmark.BookmarkId);
            return new ResponseModel(null, "Bookmark deleted successfully.", true, 200);
        }
        catch (Exception ex)
        {
            return new ResponseModel(null, $"Error deleting bookmark: {ex.Message}", false, 500);
        }
    }
}