using AutoMapper;
using RouteService.Application.DTOs;
using RouteService.Application.DTOs.Reacts;
using RouteService.Application.DTOs.Routes;
using RouteService.Domain.Entities;

namespace RouteService.Application.Interfaces.Services;

public interface IReactService
{
  
    // Route
    Task<ResponseModel> GetAllRouteReactsAsync(
        int routeId,
        int pageNumber = 1, 
        int rowsPerPage = 10, 
        string? filterField = null, 
        string? filterValue = null,
        string? sortField = null, 
        bool sortDesc = false);
    
    Task<ResponseModel> CreateReactRouteAsync(int cyclistId, int routeId);
    
    Task<ResponseModel> DeleteReactRouteAsync(int routeId, int reactId);
    
    // Comment
    Task<ResponseModel> GetAllCommentReactsAsync(
        int commentId,
        int pageNumber = 1, 
        int rowsPerPage = 10, 
        string? filterField = null, 
        string? filterValue = null,
        string? sortField = null, 
        bool sortDesc = false);

    Task<ResponseModel> CreateReactCommentAsync(int cyclistId, int commentId);
    
    Task<ResponseModel> DeleteReactCommentAsync(int commentId, int reactId);
}

public class ReactService : IReactService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ReactService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }


    /// <summary>
    /// Get all reactions for a comment in route with pagination & filtering
    /// </summary>
    public async Task<ResponseModel> GetAllCommentReactsAsync(
        int commentId, int pageNumber = 1, int rowsPerPage = 10,
        string? filterField = null, string? filterValue = null,
        string? sortField = null, bool sortDesc = false)
    {
        try
        {
            var filters = new Dictionary<string, string>
                { { "TargetId", commentId.ToString() }, { "TargetType", "comment" } };

            if (!string.IsNullOrEmpty(filterField) && !string.IsNullOrEmpty(filterValue))
            {
                filters.Add(filterField, filterValue);
            }

            var (reactions, totalCount) = await _unitOfWork.ReactionRepository.GetAllAsync(
                pageNumber, rowsPerPage, sortField, sortDesc, filters);

            var enumerable = reactions.ToList();
            if (!enumerable.Any())
                return new ResponseModel(null, "No reactions found for this comment.", false, 404);

            return new ResponseModel(_mapper.Map<List<ReactViewModel>>(enumerable), "Reactions retrieved successfully.",
                true, 200);
        }
        catch (Exception ex)
        {
            return new ResponseModel(null, $"Error retrieving reactions: {ex.Message}", false, 500);
        }
    }


    /// <summary>
    /// Get all reactions for a route with pagination & filtering
    /// </summary>
    public async Task<ResponseModel> GetAllRouteReactsAsync(
        int routeId, int pageNumber = 1, int rowsPerPage = 10,
        string? filterField = null, string? filterValue = null,
        string? sortField = null, bool sortDesc = false)
    {
        try
        {
            var filters = new Dictionary<string, string>
                { { "TargetId", routeId.ToString() }, { "TargetType", "route" } };

            if (!string.IsNullOrEmpty(filterField) && !string.IsNullOrEmpty(filterValue))
            {
                filters.Add(filterField, filterValue);
            }

            var (reactions, totalCount) = await _unitOfWork.ReactionRepository.GetAllAsync(
                pageNumber, rowsPerPage, sortField, sortDesc, filters);

            var enumerable = reactions.ToList();
            if (!enumerable.Any())
                return new ResponseModel(null, "No reactions found for this route.", false, 404);

            return new ResponseModel(_mapper.Map<List<ReactViewModel>>(enumerable), "Reactions retrieved successfully.",
                true, 200);
        }
        catch (Exception ex)
        {
            return new ResponseModel(null, $"Error retrieving reactions: {ex.Message}", false, 500);
        }
    }



    /// <summary>
    /// Add a like (reaction) to a route , prevent duplicate reactions.
    /// </summary>
    public async Task<ResponseModel> CreateReactRouteAsync(int cyclistId, int routeId)
    {
        try
        {
            // Check if the target entity exists (optional but recommended)
            var exists = await _unitOfWork.RouteRepository.ExistsAsync(routeId);
            if (!exists)
            {
                return new ResponseModel(null, $"The specified {routeId} does not exist.", false, 404);
            }

            // Prevent duplicate reactions
            var existingReact = await _unitOfWork.ReactionRepository.GetByFilterAsync(
                new Dictionary<string, string>
                {
                    { "CyclistId", cyclistId.ToString() },
                    { "TargetId", routeId.ToString() },
                    { "TargetType", "route" }
                });

            if (existingReact != null)
                return new ResponseModel(null, "Reaction already exists.", false, 409);

            var newReaction = new Reaction
            {
                CyclistId = cyclistId,
                TargetId = routeId,
                TargetType = "route",
                CreatedAt = DateTime.UtcNow
            };

            var createdReaction = await _unitOfWork.ReactionRepository.CreateAsync(newReaction);
            return new ResponseModel(_mapper.Map<ReactViewModel>(createdReaction), "Reaction added successfully.", true,
                201);
        }
        catch (Exception ex)
        {
            return new ResponseModel(null, $"Error adding reaction: {ex.Message}", false, 500);
        }
    }

    /// <summary>
    /// Add a like (reaction) to a comment, prevent duplicate reactions.
    /// </summary>
    public async Task<ResponseModel> CreateReactCommentAsync(int cyclistId, int commentId)
    {
        try
        {
            // Check if the target entity exists (optional but recommended)
            var exists = await _unitOfWork.RouteCommentRepository.ExistsAsync(commentId);
            if (!exists)
            {
                return new ResponseModel(null, $"The specified {commentId} does not exist.", false, 404);
            }

            // Prevent duplicate reactions
            var existingReact = await _unitOfWork.ReactionRepository.GetByFilterAsync(
                new Dictionary<string, string>
                {
                    { "CyclistId", cyclistId.ToString() },
                    { "TargetId", commentId.ToString() },
                    { "TargetType", "comment" }
                });

            if (existingReact != null)
                return new ResponseModel(null, "Reaction already exists.", false, 409);

            var newReaction = new Reaction
            {
                CyclistId = cyclistId,
                TargetId = commentId,
                TargetType = "comment",
                CreatedAt = DateTime.UtcNow
            };

            var createdReaction = await _unitOfWork.ReactionRepository.CreateAsync(newReaction);
            return new ResponseModel(_mapper.Map<ReactViewModel>(createdReaction), "Reaction added successfully.", true,
                201);
        }
        catch (Exception ex)
        {
            return new ResponseModel(null, $"Error adding reaction: {ex.Message}", false, 500);
        }
    }



    /// <summary>
    /// Remove a reaction from a route
    /// </summary>
    public async Task<ResponseModel> DeleteReactRouteAsync(int routeId, int reactId)
    {
        try
        {
            // Check if the target entity exists (optional but recommended)
            var exists = await _unitOfWork.RouteRepository.ExistsAsync(routeId);
            if (!exists)
            {
                return new ResponseModel(null, $"The specified {routeId} does not exist.", false, 404);
            }

            var existingReact = await _unitOfWork.ReactionRepository.GetByIdAsync(reactId);

            if (existingReact == null)
                return new ResponseModel(null, "Reaction not found.", false, 404);

            await _unitOfWork.ReactionRepository.DeleteAsync(existingReact.ReactionId);
            return new ResponseModel(null, "Reaction removed successfully.", true, 200);
        }
        catch (Exception ex)
        {
            return new ResponseModel(null, $"Error removing reaction: {ex.Message}", false, 500);
        }
    }

    /// <summary>
    /// Remove a reaction from a comment
    /// </summary>
    public async Task<ResponseModel> DeleteReactCommentAsync(int commentId, int reactId)
    {
        try
        {
            // Check if the target entity exists (optional but recommended)
            var exists = await _unitOfWork.RouteRepository.ExistsAsync(commentId);
            if (!exists)
            {
                return new ResponseModel(null, $"The specified {commentId} does not exist.", false, 404);
            }

            var existingReact = await _unitOfWork.ReactionRepository.GetByIdAsync(reactId);

            if (existingReact == null)
                return new ResponseModel(null, "Reaction not found.", false, 404);

            await _unitOfWork.ReactionRepository.DeleteAsync(existingReact.ReactionId);
            return new ResponseModel(null, "Reaction removed successfully.", true, 200);
        }
        catch (Exception ex)
        {
            return new ResponseModel(null, $"Error removing reaction: {ex.Message}", false, 500);
        }
    }
}