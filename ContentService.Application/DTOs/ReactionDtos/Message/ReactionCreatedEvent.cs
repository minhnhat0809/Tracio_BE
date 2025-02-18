namespace ContentService.Application.DTOs.ReactionDtos.Message;

public class ReactionCreatedEvent(int reactionId, string entityType)
{
    public int ReactionId { get; set; } = reactionId;

    public string EntityType { get; set; } = entityType;
}