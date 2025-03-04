using ContentService.Application.DTOs.ReactionDtos.Message;
using ContentService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Dtos;
using System.Linq.Expressions;
using ContentService.Domain.Entities;

namespace ContentService.Application.Commands.Handlers;

public class DeleteReactionCommandHandler(
    IReactionRepo reactionRepo,
    IRabbitMqProducer rabbitMqProducer,
    ILogger<DeleteReactionCommandHandler> logger) : IRequestHandler<DeleteReactionCommand, ResponseDto>
{
    private readonly IReactionRepo _reactionRepo = reactionRepo;
    private readonly IRabbitMqProducer _rabbitMqProducer = rabbitMqProducer;
    private readonly ILogger<DeleteReactionCommandHandler> _logger = logger;

    public async Task<ResponseDto> Handle(DeleteReactionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("📌 DeleteReactionCommand started. UserId: {UserId}, EntityId: {EntityId}, EntityType: {EntityType}", 
                request.UserRequestId, request.EntityId, request.EntityType);

            var (reactionId, entityId, entityType) = await GetReactionAsync(request);
            if (reactionId == null)
            {
                _logger.LogWarning("❌ Reaction not found. UserId: {UserId}, EntityId: {EntityId}, EntityType: {EntityType}", 
                    request.UserRequestId, request.EntityId, request.EntityType);
                return ResponseDto.NotFound("Reaction not found");
            }

            var isSucceed = await _reactionRepo.DeleteAsync(reactionId.Value);
            if (!isSucceed)
            {
                _logger.LogError("❌ Failed to delete reaction. ReactionId: {ReactionId}, UserId: {UserId}, EntityId: {EntityId}, EntityType: {EntityType}", 
                    reactionId, request.UserRequestId, request.EntityId, request.EntityType);
                return ResponseDto.InternalError("Failed to delete reaction.");
            }

            _logger.LogInformation("✅ Reaction deleted successfully! ReactionId: {ReactionId}, UserId: {UserId}, EntityId: {EntityId}, EntityType: {EntityType}", 
                reactionId, request.UserRequestId, request.EntityId, request.EntityType);

            await _rabbitMqProducer.SendAsync(new ReactionDeleteEvent(entityId!.Value, entityType), "content.reaction.deleted", cancellationToken);

            return ResponseDto.DeleteSuccess("Reaction deleted successfully!");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "🚨 Exception while deleting reaction. UserId: {UserId}, EntityId: {EntityId}, EntityType: {EntityType}", 
                request.UserRequestId, request.EntityId, request.EntityType);
            return ResponseDto.InternalError("Something went wrong while deleting the reaction.");
        }
    }

    private async Task<(int? reactionId, int? entityId, string entityType)> GetReactionAsync(DeleteReactionCommand request)
    {
        _logger.LogInformation("🔄 Fetching reaction for deletion. UserId: {UserId}, EntityId: {EntityId}, EntityType: {EntityType}", 
            request.UserRequestId, request.EntityId, request.EntityType);

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

        if (reaction != null)
        {
            _logger.LogInformation("✅ Reaction found. ReactionId: {ReactionId}, EntityId: {EntityId}, EntityType: {EntityType}", 
                reaction.ReactionId, reaction.EntityId, entityType);
            return (reaction.ReactionId, reaction.EntityId, entityType);
        }
        
        _logger.LogWarning("❌ Reaction not found for deletion. EntityType: {EntityType}", entityType);
        return (null, null, entityType);
    }
}
