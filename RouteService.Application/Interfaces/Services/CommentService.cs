using AutoMapper;
using RouteService.Application.DTOs;
using RouteService.Application.DTOs.RouteComment;
using RouteService.Application.DTOs.Routes;
using RouteService.Domain.Entities;

namespace RouteService.Application.Interfaces.Services;

public interface ICommentService
{
    
    // Get comments (Level 1) by Route ID
    Task<ResponseModel> GetCommentsByRouteIdAsync(
        int routeId, int pageNumber = 1, int rowsPerPage = 10, 
        string? filterField = null, string? filterValue = null, 
        string? sortField = null, bool sortDesc = false
    );

    // Get replies (Level 2) for a specific comment
    Task<ResponseModel> GetRepliesByCommentIdAsync(
        int commentId, int pageNumber = 1, int rowsPerPage = 10, 
        string? filterField = null, string? filterValue = null, 
        string? sortField = null, bool sortDesc = false
    );

    // Create a new comment or reply
    Task<ResponseModel> CreateCommentAsync(int routeId, CommentCreateRequestModel request);
    Task<ResponseModel> CreateReplyAsync(int commentId, CommentCreateRequestModel request);

    // Update an existing comment or reply
    Task<ResponseModel> UpdateCommentAsync(int commentId, CommentUpdateRequestModel request);
    Task<ResponseModel> UpdateReplyAsync(int commentId, int replyId, CommentUpdateRequestModel request);

    // Delete a comment or reply
    Task<ResponseModel> DeleteCommentAsync(int commentId);
    Task<ResponseModel> DeleteReplyAsync(int commentId, int replyId);
}


public class CommentService : ICommentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CommentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// Get all comments for a specific route (Level 1)
    /// </summary>
    public async Task<ResponseModel> GetCommentsByRouteIdAsync(
        int routeId, int pageNumber = 1, int rowsPerPage = 10,
        string? filterField = null, string? filterValue = null,
        string? sortField = null, bool sortDesc = false)
    {
        try
        {
            var filters = new Dictionary<string, string> { { "RouteId", routeId.ToString() } };

            var (comments, totalCount) = await _unitOfWork.RouteCommentRepository.GetAllAsync(
                pageNumber, rowsPerPage, sortField, sortDesc, filters, "Route");

            var level1Comments = comments.Where(c => c.ParentCommentId == null).ToList();
        
            return new ResponseModel(_mapper.Map<List<CommentViewModel>>(level1Comments), "Comments retrieved successfully.", true, 200);
        }
        catch (Exception ex)
        {
            return new ResponseModel(null, $"Error retrieving comments: {ex.Message}", false, 500);
        }
    }


    /// <summary>
    /// Get all replies for a specific comment (Level 2)
    /// </summary>
    public async Task<ResponseModel> GetRepliesByCommentIdAsync(
        int commentId, int pageNumber = 1, int rowsPerPage = 10,
        string? filterField = null, string? filterValue = null,
        string? sortField = null, bool sortDesc = false)
    {
        try
        {
            var comment = await _unitOfWork.RouteCommentRepository.GetByIdAsync(commentId);
            if (comment == null)
                return new ResponseModel(null, "Comment not found.", false, 404);

            var filters = new Dictionary<string, string> { { "ParentCommentId", commentId.ToString() } };

            var (replies, totalCount) = await _unitOfWork.RouteCommentRepository.GetAllAsync(
                pageNumber, rowsPerPage, sortField, sortDesc, filters);

            return new ResponseModel(_mapper.Map<List<ReplyViewModel>>(replies), "Replies retrieved successfully.", true, 200);
        }
        catch (Exception ex)
        {
            return new ResponseModel(null, $"Error retrieving replies: {ex.Message}", false, 500);
        }
    }

    /*/// <summary>
    /// Create a comment (Level 1) or reply (Level 2)
    /// </summary>
    /*public async Task<ResponseModel> CreateCommentOrReplyAsync(CommentCreateRequestModel request)
    {
        try
        {
            // Validate request
            if (request.ParentCommentId.HasValue)
            {
                // If it's a reply, check if the parent comment exists
                var parentComment = await _unitOfWork.RouteCommentRepository.GetByIdAsync(request.ParentCommentId.Value);
                if (parentComment == null)
                {
                    return new ResponseModel(null, "Parent comment does not exist.", false, 400);
                }
            }

            var newComment = new RouteComment
            {
                RouteId = request.RouteId,
                CyclistId = request.CyclistId,
                CommentContent = request.Content,
                ParentCommentId = request.ParentCommentId,
                CreatedAt = DateTime.UtcNow
            };

            var createdComment = await _unitOfWork.RouteCommentRepository.CreateAsync(newComment);
            return new ResponseModel(createdComment, "Comment or reply created successfully.", true, 201);
        }
        catch (Exception ex)
        {
            return new ResponseModel(null, $"Error creating comment: {ex.Message}", false, 500);
        }
    }#1#

    /// <summary>
    /// Update a comment or reply
    /// </summary>
    /*public async Task<ResponseModel> UpdateCommentAsync(int commentId, CommentUpdateRequestModel request)
    {
        try
        {
            var comment = await _unitOfWork.RouteCommentRepository.GetByIdAsync(commentId);
            if (comment == null)
                return new ResponseModel(null, "Comment not found.", false, 404);

            comment.CommentContent = request.Content;
            await _unitOfWork.RouteCommentRepository.UpdateAsync(comment);

            return new ResponseModel(comment, "Comment updated successfully.", true, 200);
        }
        catch (Exception ex)
        {
            return new ResponseModel(null, $"Error updating comment: {ex.Message}", false, 500);
        }
    }#1#

    /// <summary>
    /// Delete a comment or reply
    /// </summary>
    /*public async Task<ResponseModel> DeleteCommentAsync(int commentId)
    {
        try
        {
            var comment = await _unitOfWork.RouteCommentRepository.GetByIdAsync(commentId);
            if (comment == null)
                return new ResponseModel(null, "Comment not found.", false, 404);

            await _unitOfWork.RouteCommentRepository.DeleteAsync(commentId);
            return new ResponseModel(null, "Comment deleted successfully.", true, 200);
        }
        catch (Exception ex)
        {
            return new ResponseModel(null, $"Error deleting comment: {ex.Message}", false, 500);
        }
    }#1#*/
    
    /// <summary>
    /// Create a comment (Level 1) 
    /// </summary>
    public async Task<ResponseModel> CreateCommentAsync(int routeId, CommentCreateRequestModel request)
    {
        try
        {
            // Kiểm tra route có tồn tại không
            var routeExists = await _unitOfWork.RouteRepository.ExistsAsync(routeId);
            if (!routeExists)
                return new ResponseModel(null, "Route does not exist.", false, 400);

            var newComment = new RouteComment
            {
                RouteId = routeId,
                CyclistId = request.CyclistId,
                CommentContent = request.Content,
                ParentCommentId = null, // Đây là comment gốc (Level 1)
                CreatedAt = DateTime.UtcNow
            };

            var createdComment = await _unitOfWork.RouteCommentRepository.CreateAsync(newComment);
            return new ResponseModel(createdComment, "Comment created successfully.", true, 201);
        }
        catch (Exception ex)
        {
            return new ResponseModel(null, $"Error creating comment: {ex.Message}", false, 500);
        }
    }
    
    /// <summary>
    /// Create a Reply (Level 2)
    /// </summary>
    public async Task<ResponseModel> CreateReplyAsync(int commentId, CommentCreateRequestModel request)
    {
        try
        {
            // Kiểm tra xem comment cha có tồn tại không
            var parentComment = await _unitOfWork.RouteCommentRepository.GetByIdAsync(commentId);
            if (parentComment == null)
                return new ResponseModel(null, "Parent comment does not exist.", false, 400);

            // Giới hạn chỉ cho phép reply cấp 2 (không cho phép reply vào reply)
            if (parentComment.ParentCommentId.HasValue)
                return new ResponseModel(null, "Cannot reply to a reply. Only two levels are allowed.", false, 400);

            var newReply = new RouteComment
            {
                RouteId = parentComment.RouteId, // Reply thuộc cùng route với parent
                CyclistId = request.CyclistId,
                ParentCommentId = commentId, // Gắn Parent ID
                CommentContent = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            var createdReply = await _unitOfWork.RouteCommentRepository.CreateAsync(newReply);

            // parent comment count +1
            parentComment.CommentCounts += 1;
            await _unitOfWork.RouteCommentRepository.UpdateAsync(parentComment);
            return new ResponseModel(createdReply, "Reply created successfully.", true, 201);
        }
        catch (Exception ex)
        {
            return new ResponseModel(null, $"Error creating reply: {ex.Message}", false, 500);
        }
    }
    
    /// <summary>
    /// Update a comment 
    /// </summary>
    public async Task<ResponseModel> UpdateCommentAsync(int commentId, CommentUpdateRequestModel request)
    {
        try
        {
            var comment = await _unitOfWork.RouteCommentRepository.GetByIdAsync(commentId);
            if (comment == null || comment.ParentCommentId != null) // Chỉ cập nhật comment gốc
                return new ResponseModel(null, "Comment not found or is a reply.", false, 404);

            // Kiểm tra quyền cập nhật
            if (comment.CyclistId != request.CyclistId)
                return new ResponseModel(null, "Unauthorized to update this comment.", false, 403);

            comment.CommentContent = request.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.RouteCommentRepository.UpdateAsync(comment);
            return new ResponseModel(comment, "Comment updated successfully.", true, 200);
        }
        catch (Exception ex)
        {
            return new ResponseModel(null, $"Error updating comment: {ex.Message}", false, 500);
        }
    }

    /// <summary>
    /// Update a reply
    /// </summary>
    public async Task<ResponseModel> UpdateReplyAsync(int commentId, int replyId, CommentUpdateRequestModel request)
    {
        try
        {
            var reply = await _unitOfWork.RouteCommentRepository.GetByIdAsync(replyId);
            if (reply == null || reply.ParentCommentId != commentId) // Chỉ cập nhật reply
                return new ResponseModel(null, "Reply not found or does not belong to the specified comment.", false, 404);

            // Kiểm tra quyền cập nhật
            if (reply.CyclistId != request.CyclistId)
                return new ResponseModel(null, "Unauthorized to update this reply.", false, 403);

            reply.CommentContent = request.Content;
            reply.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.RouteCommentRepository.UpdateAsync(reply);
            return new ResponseModel(reply, "Reply updated successfully.", true, 200);
        }
        catch (Exception ex)
        {
            return new ResponseModel(null, $"Error updating reply: {ex.Message}", false, 500);
        }
    }

    /// <summary>
    /// Delete a comment 
    /// </summary>
    public async Task<ResponseModel> DeleteCommentAsync(int commentId)
    {
        try
        {
            var comment = await _unitOfWork.RouteCommentRepository.GetByIdAsync(commentId);
            if (comment == null || comment.ParentCommentId != null) // Chỉ xóa comment cấp 1
                return new ResponseModel(null, "Comment not found or is a reply.", false, 404);

            // Kiểm tra xem có reply không
            /*var hasReplies = await _unitOfWork.RouteCommentRepository.ExistsAsync(commentId);
            if (hasReplies)
                return new ResponseModel(null, "Cannot delete comment with replies.", false, 400);
                */

            await _unitOfWork.RouteCommentRepository.DeleteAsync(commentId);
            return new ResponseModel(null, "Comment deleted successfully.", true, 200);
        }
        catch (Exception ex)
        {
            return new ResponseModel(null, $"Error deleting comment: {ex.Message}", false, 500);
        }
    }

    /// <summary>
    /// Delete a reply
    /// </summary>
    public async Task<ResponseModel> DeleteReplyAsync(int commentId, int replyId)
    {
        try
        {
            var reply = await _unitOfWork.RouteCommentRepository.GetByIdAsync(replyId);
            if (reply == null || reply.ParentCommentId != commentId)
                return new ResponseModel(null, "Reply not found or does not belong to the specified comment.", false, 404);

            await _unitOfWork.RouteCommentRepository.DeleteAsync(replyId);
            return new ResponseModel(null, "Reply deleted successfully.", true, 200);
        }
        catch (Exception ex)
        {
            return new ResponseModel(null, $"Error deleting reply: {ex.Message}", false, 500);
        }
    }



}

