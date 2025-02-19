using ContentService.Application.DTOs.ReactionDtos.Message;
using ContentService.Application.Interfaces;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class DeleteReactionCommandHandler(
    IReactionRepo reactionRepo, 
    ICommentRepo commentRepo, 
    IReplyRepo replyRepo, 
    IBlogRepo blogRepo,
    IRabbitMqProducer rabbitMqProducer) : IRequestHandler<DeleteReactionCommand, ResponseDto>
{
    private readonly IReactionRepo _reactionRepo = reactionRepo;
    
    private readonly ICommentRepo _commentRepo = commentRepo;
    
    private readonly IReplyRepo _replyRepo = replyRepo;
    
    private readonly IBlogRepo _blogRepo = blogRepo;
    
    private readonly IRabbitMqProducer _rabbitMqProducer = rabbitMqProducer;
    
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
                await _rabbitMqProducer.PublishAsync(new ReactionDeleteEvent(reaction.CommentId.Value, "comment"), "content_deleted", cancellationToken);
            }
            else if (reaction.BlogId.HasValue)
            {
                await _rabbitMqProducer.PublishAsync(new ReactionDeleteEvent(reaction.BlogId.Value, "blog"), "content_deleted", cancellationToken);
            }
            else if (reaction.ReplyId.HasValue)
            {
                await _rabbitMqProducer.PublishAsync(new ReactionDeleteEvent(reaction.ReplyId.Value, "reply"), "content_deleted", cancellationToken);
            }
            
            return ResponseDto.DeleteSuccess(null, "Reaction deleted successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}