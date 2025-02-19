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
            
            // update reaction count
            if (reaction.CommentId.HasValue)
            {
                await _commentRepo.UpdateFieldsAsync(c => c.CommentId == reaction.CommentId.Value,
                    c => c.SetProperty(cc => cc.LikesCount, cc => cc.LikesCount - 1));
            }
            else if (reaction.BlogId.HasValue)
            {
                await _blogRepo.UpdateFieldsAsync(b => b.BlogId == reaction.BlogId.Value,
                    b => b.SetProperty(bl => bl.ReactionsCount, bl => bl.ReactionsCount - 1));
            }
            else if (reaction.ReplyId.HasValue)
            {
                await _replyRepo.UpdateFieldsAsync(r => r.ReplyId == reaction.ReplyId.Value,
                    r => r.SetProperty(rr => rr.LikesCount, rr => rr.LikesCount - 1));
            }
            
            return ResponseDto.DeleteSuccess(null, "Reaction deleted successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}