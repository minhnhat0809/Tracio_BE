using ContentService.Application.Interfaces;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class DeleteReactionCommandHandler(
    IReactionRepo reactionRepo, 
    ICommentRepo commentRepo, 
    IReplyRepo replyRepo, 
    IBlogRepo blogRepo) : IRequestHandler<DeleteReactionCommand, ResponseDto>
{
    private readonly IReactionRepo _reactionRepo = reactionRepo;
    
    private readonly ICommentRepo _commentRepo = commentRepo;
    
    private readonly IReplyRepo _replyRepo = replyRepo;
    
    private readonly IBlogRepo _blogRepo = blogRepo;
    
    public async Task<ResponseDto> Handle(DeleteReactionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // fetch reaction in db
            var reaction = await _reactionRepo.GetByIdAsync(c => c.ReactionId == request.ReactionId, r => new
            {
                r.ReplyId,
                r.BlogId,
                r.CommentId
            });
            if (reaction == null) return ResponseDto.NotFound("Reaction not found");

            // delete reaction
            var isSucceed = await _reactionRepo.DeleteAsync(request.ReactionId);
            if(!isSucceed) return ResponseDto.InternalError("Failed to delete reaction.");
            
            if (reaction.CommentId.HasValue)
            {
                await _commentRepo.DecrementReactionCount(reaction.CommentId.Value);
            }
            else if (reaction.BlogId.HasValue)
            {
                await _blogRepo.DecrementReactionCount(reaction.BlogId.Value);
            }
            else if (reaction.ReplyId.HasValue)
            {
                await _replyRepo.DecrementReactionCount(reaction.ReplyId.Value);
            }
            
            return ResponseDto.DeleteSuccess(null, "Reaction deleted successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}