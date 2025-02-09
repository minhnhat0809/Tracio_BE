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
            // check comment in db
            var isCommentExisted = await _commentRepo.ExistsAsync(c => c.CommentId == request.CommentId);
            if(!isCommentExisted) return ResponseDto.NotFound("Comment not found");
            
            // count
            var total = await _reactionRepo.CountAsync(c => c.CommentId == request.CommentId);
            
            var reactionDtos = await _reactionRepo.FindAsync(r => r.CommentId == request.CommentId, 
                r => new ReactionDto
                {
                    UserId = r.CyclistId,
                    UserName = r.CyclistName,
                    CreatedAt = r.CreatedAt
                });
            
            return ResponseDto.GetSuccess(
            new {
                reations = reactionDtos,
                total
            }, "Reactions retrieved successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}