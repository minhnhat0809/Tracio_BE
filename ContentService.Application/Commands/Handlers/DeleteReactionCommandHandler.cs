using ContentService.Application.DTOs.ReactionDtos.Message;
using ContentService.Application.Interfaces;
using MediatR;
using Shared.Dtos;
using System.Linq.Expressions;
using ContentService.Domain.Entities;

namespace ContentService.Application.Commands.Handlers;

public class DeleteReactionCommandHandler(
    IReactionRepo reactionRepo,
    IRabbitMqProducer rabbitMqProducer) : IRequestHandler<DeleteReactionCommand, ResponseDto>
{
    private readonly IReactionRepo _reactionRepo = reactionRepo;
    private readonly IRabbitMqProducer _rabbitMqProducer = rabbitMqProducer;

    public async Task<ResponseDto> Handle(DeleteReactionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var (reactionId, entityId, entityType) = await GetReactionAsync(request);
            if (reactionId == null) return ResponseDto.NotFound("Reaction not found");

            // Delete reaction
            var isSucceed = await _reactionRepo.DeleteAsync(reactionId.Value);
            if (!isSucceed) return ResponseDto.InternalError("Failed to delete reaction.");

            // Publish event
            await _rabbitMqProducer.SendAsync(new ReactionDeleteEvent(entityId!.Value, entityType), "content.reaction.deleted", cancellationToken);

            return ResponseDto.DeleteSuccess("Reaction deleted successfully!");
        }
        catch (Exception e)
        {
            Console.WriteLine($"[DeleteReaction] ERROR: {e.Message}\n{e.StackTrace}");
            return ResponseDto.InternalError(e.Message);
        }
    }

    private async Task<(int? reactionId, int? entityId, string entityType)> GetReactionAsync(DeleteReactionCommand request)
    {
        return request.EntityType.ToLower() switch
        {
            "comment" => await GetReactionDetails(r => r.CommentId == request.EntityId && r.CyclistId == request.UserRequestId, "comment"),
            "blog" => await GetReactionDetails(r => r.BlogId == request.EntityId && r.CyclistId == request.UserRequestId, "blog"),
            "reply" => await GetReactionDetails(r => r.ReplyId == request.EntityId && r.CyclistId == request.UserRequestId, "reply"),
            _ => (null, null, request.EntityType)
        };
    }

    private async Task<(int? reactionId, int? entityId, string entityType)> GetReactionDetails(
        Expression<Func<Reaction, bool>> predicate, string entityType)
    {
        var reaction = await _reactionRepo.GetByIdAsync(predicate, r => new { r.ReactionId, EntityId = r.CommentId ?? r.BlogId ?? r.ReplyId });
        return reaction != null ? (reaction.ReactionId, reaction.EntityId, entityType) : (null, null, entityType);
    }
}
