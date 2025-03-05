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
    
    // Get replies in reply (Level 3) for a specific comment
    Task<ResponseModel> GetRepliesByReplyIdAsync(
        int replyId, int pageNumber = 1, int rowsPerPage = 10, 
        string? filterField = null, string? filterValue = null, 
        string? sortField = null, bool sortDesc = false
    );

    // Create a new comment or reply 
    Task<ResponseModel> CreateCommentAsync(int cyclistId, int routeId, CommentCreateRequestModel request);
    Task<ResponseModel> CreateReplyAsync(int cyclistId, int commentId, CommentCreateRequestModel request);
    Task<ResponseModel> CreateNestedReplyAsync(int cyclistId, int replyId, CommentCreateRequestModel request);

    // Update an existing comment or reply
    Task<ResponseModel> UpdateCommentAsync(int cyclistId, int commentId, CommentUpdateRequestModel request);
    Task<ResponseModel> UpdateReplyAsync(int cyclistId, int commentId, int replyId, CommentUpdateRequestModel request);
    Task<ResponseModel> UpdateNestedReplyAsync(int cyclistId, int replyId, int nestedReplyId, CommentUpdateRequestModel request);

    // Delete a comment or reply
    Task<ResponseModel> DeleteCommentAsync(int cyclistId, int commentId);
    Task<ResponseModel> DeleteReplyAsync(int cyclistId, int commentId, int replyId);
    Task<ResponseModel> DeleteNestedReplyAsync(int cyclistId, int replyId, int nestedReplyId);
}


public class CommentService : ICommentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    public CommentService(IUnitOfWork unitOfWork, IMapper mapper, IUserRepository userRepository)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userRepository = userRepository;
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
    
    /// <summary>
    /// Get all replies for a specific reply (Level 3)
    /// </summary>
    public async Task<ResponseModel> GetRepliesByReplyIdAsync(
        int replyId, int pageNumber = 1, int rowsPerPage = 10,
        string? filterField = null, string? filterValue = null,
        string? sortField = null, bool sortDesc = false)
    {
        try
        {
            // get reply level 2
            var reply = await _unitOfWork.RouteCommentRepository.GetByIdAsync(replyId);
            if (reply == null)
                return new ResponseModel(null, "Comment not found.", false, 404);

            var filters = new Dictionary<string, string> { { "ParentCommentId", reply.CommentId.ToString() } };

            var (replies, totalCount) = await _unitOfWork.RouteCommentRepository.GetAllAsync(
                pageNumber, rowsPerPage, sortField, sortDesc, filters);

            return new ResponseModel(
                (_mapper.Map<List<ReplyViewModel>>(replies), totalCount), "Replies retrieved successfully.", true, 200);
        }
        catch (Exception ex)
        {
            return new ResponseModel(null, $"Error retrieving replies: {ex.Message}", false, 500);
        }
    }
    
    /// <summary>
    /// Create a comment (Level 1) 
    /// </summary>
    public async Task<ResponseModel> CreateCommentAsync(int cyclistId, int routeId, CommentCreateRequestModel request)
    {
        try
        {
            // check user
            var user = await _userRepository.ValidateUserAsync(cyclistId);
            if (!user)
                return new ResponseModel(null, $"The user specified {cyclistId} does not exist.", false, 404);
            
            // check route
            var routeExists = await _unitOfWork.RouteRepository.ExistsAsync(routeId);
            if (!routeExists)
                return new ResponseModel(null, "Route does not exist.", false, 400);

            var newComment = new RouteComment
            {
                RouteId = routeId,
                CyclistId = cyclistId,
                CommentContent = request.Content,
                ParentCommentId = null, // comment (Level 1)
                CreatedAt = DateTime.UtcNow
            };

            var createdComment = await _unitOfWork.RouteCommentRepository.CreateAsync(newComment);
            // publish to rabbitMq add queue for route_increase_comment_count_queue
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
    public async Task<ResponseModel> CreateReplyAsync(int cyclistId, int commentId, CommentCreateRequestModel request)
    {
        try
        {
            // check user
            var user = await _userRepository.ValidateUserAsync(cyclistId);
            if (!user)
                return new ResponseModel(null, $"The user specified {cyclistId} does not exist.", false, 404);
            
            // check parent comment (comment lv1)
            var parentComment = await _unitOfWork.RouteCommentRepository.GetByIdAsync(commentId);
            if (parentComment == null)
                return new ResponseModel(null, "Parent comment does not exist.", false, 400);
            
            var newReply = new RouteComment
            {
                RouteId = parentComment.RouteId, // at same route 
                CyclistId = cyclistId,
                ParentCommentId = commentId, 
                CommentContent = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            var createdReply = await _unitOfWork.RouteCommentRepository.CreateAsync(newReply);

            // parent comment count +1 (comment lv1)
            parentComment.CommentCounts += 1;
            await _unitOfWork.RouteCommentRepository.UpdateAsync(parentComment);
            
            // publish to rabbitMq add queue for route_increase_comment_count_queue && comment_increase_comment_count_queue
            
            return new ResponseModel(createdReply, "Reply created successfully.", true, 201);
        }
        catch (Exception ex)
        {
            return new ResponseModel(null, $"Error creating reply: {ex.Message}", false, 500);
        }
    }
    
    /// <summary>
    /// Create a Nested Reply (Level 3)
    /// </summary>
    public async Task<ResponseModel> CreateNestedReplyAsync(int cyclistId, int replyId, CommentCreateRequestModel request)
    {
        try
        {
            // check user
            var user = await _userRepository.ValidateUserAsync(cyclistId);
            if (!user)
                return new ResponseModel(null, $"The user specified {cyclistId} does not exist.", false, 404);
            
            // check parent comment 
            var parentComment = await _unitOfWork.RouteCommentRepository.GetByIdAsync(replyId);
            if (parentComment == null)
                return new ResponseModel(null, "Comment does not exist.", false, 400);

            int nestedReplyParentId = replyId; 
            RouteComment? grandParentComment = null;
            // check grandParent
            if (parentComment.ParentCommentId.HasValue)
            {
                grandParentComment = await _unitOfWork.RouteCommentRepository.GetByIdAsync(parentComment.ParentCommentId.Value);
                // if exist
                if (grandParentComment != null && grandParentComment.ParentCommentId.HasValue)
                {
                    // if you are reply in nested reply (lv4): newReply.ParentComment == replyId.ParentComment
                    nestedReplyParentId = parentComment.CommentId;
                } 
                // what if this comment was deleted ? HELP-HELP-HELP-HELP-HELP-HELP-HELP-HELP-HELP-HELP-HELP
            }
            var newNestedReply = new RouteComment
            {
                RouteId = parentComment.RouteId, 
                CyclistId = cyclistId,
                ParentCommentId = nestedReplyParentId,
                CommentContent = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            var createdReply = await _unitOfWork.RouteCommentRepository.CreateAsync(newNestedReply);

            
            /*
             * lv1: 0
             * lv2: count +1 parent comment id
             * lv3: count +1 parent comment id and count +1 grand parent comment id
             */
            // publish to rabbitMq add queue for route_increase_comment_count_queue && comment_increase_comment_count_queue && reply_increase_comment_count_queue
            if (grandParentComment != null) 
            {
                // If this is a level 3 a nested reply, count grandparent comment id and parent comment id
                parentComment.CommentCounts += 1;
                grandParentComment.CommentCounts += 1;
                await _unitOfWork.RouteCommentRepository.UpdateAsync(parentComment);
                await _unitOfWork.RouteCommentRepository.UpdateAsync(grandParentComment);
            }
            else
            {
                // This is a level 2 reply, only update level 1's CommentCounts
                parentComment.CommentCounts += 1;
                await _unitOfWork.RouteCommentRepository.UpdateAsync(parentComment);
            }

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
    public async Task<ResponseModel> UpdateCommentAsync(int cyclistId, int commentId, CommentUpdateRequestModel request)
    {
        try
        {
            // check comment
            var comment = await _unitOfWork.RouteCommentRepository.GetByIdAsync(commentId);
            if (comment == null || comment.ParentCommentId != null)
                return new ResponseModel(null, "Comment not found or is a reply.", false, 404);
            
            // check user
            if (comment.CyclistId != cyclistId)
                return new ResponseModel(null, "Unauthorized to update this comment.", false, 403);

            comment.CommentContent = request.Content;
            comment.UpdatedAt = DateTime.UtcNow;
            
            // save to redis and db also, clear data on redis

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
    public async Task<ResponseModel> UpdateReplyAsync(int cyclistId, int commentId, int replyId, CommentUpdateRequestModel request)
    {
        try
        {
            // check reply
            var reply = await _unitOfWork.RouteCommentRepository.GetByIdAsync(replyId);
            if (reply == null || reply.ParentCommentId != commentId)
                return new ResponseModel(null, "Reply not found or does not belong to the specified comment.", false, 404);

            // check user
            if (reply.CyclistId != cyclistId)
                return new ResponseModel(null, "Unauthorized to update this reply.", false, 403);

            reply.CommentContent = request.Content;
            reply.UpdatedAt = DateTime.UtcNow;

            // save to redis and db also, clear data on redis
            
            await _unitOfWork.RouteCommentRepository.UpdateAsync(reply);
            
            return new ResponseModel(reply, "Reply updated successfully.", true, 200);
        }
        catch (Exception ex)
        {
            return new ResponseModel(null, $"Error updating reply: {ex.Message}", false, 500);
        }
    }
    
    /// <summary>
    /// Update a nested reply
    /// </summary>
    public async Task<ResponseModel> UpdateNestedReplyAsync(int cyclistId, int replyId, int nestedReplyId, CommentUpdateRequestModel request)
    {
        try
        {
            // check nested reply
            var nestedReply = await _unitOfWork.RouteCommentRepository.GetByIdAsync(nestedReplyId);
            if (nestedReply == null || nestedReply.ParentCommentId != replyId) // Chỉ cập nhật reply
                return new ResponseModel(null, "Reply not found or does not belong to the specified comment.", false, 404);

            // check user
            if (nestedReply.CyclistId != cyclistId)
                return new ResponseModel(null, "Unauthorized to update this reply.", false, 403);

            nestedReply.CommentContent = request.Content;
            nestedReply.UpdatedAt = DateTime.UtcNow;

            // save to redis and db also, clear data on redis
            
            await _unitOfWork.RouteCommentRepository.UpdateAsync(nestedReply);
            
            return new ResponseModel(_mapper.Map<CommentViewModel>(nestedReply), "Reply updated successfully.", true, 200);
        }
        catch (Exception ex)
        {
            return new ResponseModel(null, $"Error updating reply: {ex.Message}", false, 500);
        }
    }

    /// <summary>
    /// Delete a comment lv1
    /// </summary>
    public async Task<ResponseModel> DeleteCommentAsync(int cyclistId, int commentId)
    {
        try
        {
            // check comment
            var comment = await _unitOfWork.RouteCommentRepository.GetByIdAsync(commentId);
            if (comment == null || comment.ParentCommentId != null)
                return new ResponseModel(null, "Comment not found or is a reply.", false, 404);

            // check user
            if (comment.CyclistId != cyclistId)
                return new ResponseModel(null, "Unauthorized to update this reply.", false, 403);

            // save to redis and db also, clear data on redis

            // this delete is delete all comment, reply and reply-of-reply
            await _unitOfWork.RouteCommentRepository.DeleteAsync(commentId);
            
            // route decrease 1 count comment unit
            var route = await _unitOfWork.RouteRepository.GetByIdAsync(comment.RouteId);
            
            if (route != null)
            {
                route.CommentCounts -= 1;
                await _unitOfWork.RouteRepository.UpdateAsync(route);
            }
            
            // publish to rabbitMq add queue for route_decrease_comment_count_queue
            
            return new ResponseModel(null, "Comment deleted successfully.", true, 200);
        }
        catch (Exception ex)
        {
            return new ResponseModel(null, $"Error deleting comment: {ex.Message}", false, 500);
        }
    }

    /// <summary>
    /// Delete a reply lv2
    /// </summary>
    public async Task<ResponseModel> DeleteReplyAsync(int cyclistId, int commentId, int replyId)
    {
        try
        {
            // check reply
            var reply = await _unitOfWork.RouteCommentRepository.GetByIdAsync(replyId);
            if (reply == null || reply.ParentCommentId != commentId)
                return new ResponseModel(null, "Reply not found or does not belong to the specified comment.", false, 404);

            // check user
            if (reply.CyclistId != cyclistId)
                return new ResponseModel(null, "Unauthorized to update this reply.", false, 403);

            
            // check parent Comment
            var comment = await _unitOfWork.RouteCommentRepository.GetByIdAsync(commentId);
            if (comment == null)
                return new ResponseModel(null, "Comment not found", false, 404);

            // save to redis and db also, clear data on redis

            // this delete is delete all comment, reply and reply-of-reply
            await _unitOfWork.RouteCommentRepository.DeleteAsync(replyId);
            
            // comment decrease 1 count comment unit and route decrease comment.countComment +1 unit
            comment.CommentCounts -= (reply.CommentCounts + 1);
            await _unitOfWork.RouteCommentRepository.UpdateAsync(comment);
            
            var route = await _unitOfWork.RouteRepository.GetByIdAsync(comment.RouteId);
            if (route != null)
            {
                route.CommentCounts -= 1;
                await _unitOfWork.RouteRepository.UpdateAsync(route);
            }
            
            return new ResponseModel(null, "Reply deleted successfully.", true, 200);
        }
        catch (Exception ex)
        {
            return new ResponseModel(null, $"Error deleting reply: {ex.Message}", false, 500);
        }
    }

    /// <summary>
    /// Delete a nested reply lv3
    /// </summary>
    public async Task<ResponseModel> DeleteNestedReplyAsync(int cyclistId, int replyId, int nestedReplyId)
    {
        try
        {
            // check nested reply
            var nestedReply = await _unitOfWork.RouteCommentRepository.GetByIdAsync(nestedReplyId);
            if (nestedReply == null || nestedReply.ParentCommentId != replyId)
                return new ResponseModel(null, "Reply not found or does not belong to the specified comment.", false, 404);

            // check user
            if (nestedReply.CyclistId != cyclistId)
                return new ResponseModel(null, "Unauthorized to update this reply.", false, 403);
            
            // check parent  
            var parentComment = await _unitOfWork.RouteCommentRepository.GetByIdAsync(replyId);
            if(parentComment == null)
                return new ResponseModel(null, "Reply not found", false, 404);
            
            // check grandParent 
            if (parentComment.ParentCommentId.HasValue)
            {
                var grandParentComment = await _unitOfWork.RouteCommentRepository.GetByIdAsync(parentComment.ParentCommentId.Value);
                // if exist
                if (grandParentComment != null && grandParentComment.ParentCommentId.HasValue)
                {
                    // if you are in nested reply (lv4)
                    // 1 unit for 3 count unit
                    parentComment.CommentCounts -= 1;
                    await _unitOfWork.RouteCommentRepository.UpdateAsync(parentComment);
                    grandParentComment.CommentCounts -= 1;
                    await _unitOfWork.RouteCommentRepository.UpdateAsync(grandParentComment);
                    var route = await _unitOfWork.RouteRepository.GetByIdAsync(parentComment.RouteId);
                    if (route != null)
                    {
                        route.CommentCounts -= 1;
                        await _unitOfWork.RouteCommentRepository.DeleteAsync(grandParentComment);
                    }
                } 
                // throw use the delete reply instead 
            }
            
            // save to redis and db also, clear data on redis

            // this delete is delete all comment, reply and reply-of-reply
            await _unitOfWork.RouteCommentRepository.DeleteAsync(nestedReplyId);
            
            return new ResponseModel(null, "Reply deleted successfully.", true, 200);
        }
        catch (Exception ex)
        {
            return new ResponseModel(null, $"Error deleting reply: {ex.Message}", false, 500);
        }
    }

}

