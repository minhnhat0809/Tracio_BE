using ContentService.Application.DTOs.ReactionDtos.ViewDtos;
using ContentService.Application.Interfaces;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Queries.Handlers;

public class GetReactionsByCommentQueryHandler(IReactionRepo reactionRepo, ICommentRepo commentRepo) : IRequestHandler<GetReactionsByCommentQuery, ResponseDto>
{
    private readonly IReactionRepo _reactionRepo = reactionRepo;
    
    private readonly ICommentRepo _commentRepo = commentRepo;
    
    public async Task<ResponseDto> Handle(GetReactionsByCommentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.CommentId <= 0) return ResponseDto.BadRequest("Comment Id is required");
            
            // check comment in db
            var isCommentExisted = await _commentRepo.ExistsAsync(c => c.CommentId == request.CommentId);
            if(!isCommentExisted) return ResponseDto.NotFound("Comment not found");
            
            var totalReactions = await _reactionRepo.CountAsync(c => c.CommentId == request.CommentId);
            
            var totalPages = (int)Math.Ceiling((double)totalReactions / request.PageSize);
            
            var reactionDtos = await _reactionRepo.FindAsync(r => r.CommentId == request.CommentId, 
                r => new ReactionDto
                {
                    CyclistId = r.CyclistId,
                    CyclistName = r.CyclistName,
                    CyclistAvatar = r.CyclistAvatar
                });
            
            return ResponseDto.GetSuccess(
            new {
                reations = reactionDtos,
                totalReactions,
                totalPages,
                hasNextPage = request.PageNumber < totalPages,
                hasPreviousPage = request.PageNumber > 1
            }, "Reactions retrieved successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}