namespace ContentService.Application.DTOs.ReactionDtos.Message;

public class ReactionCreateEvent(int entityId, string entityType)
{
    public int EntityId { get; set; } = entityId;

    public string EntityType { get; set; } = entityType;
}