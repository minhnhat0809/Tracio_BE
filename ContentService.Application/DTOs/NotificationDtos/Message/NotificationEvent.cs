namespace ContentService.Application.DTOs.NotificationDtos.Message;

public class NotificationEvent(int recipientId, int senderId, string senderName, string senderAvatar, string message, int entityId, string entityType, DateTime createdAt)
{
    public int RecipientId { get; set; } = recipientId;
    
    public int SenderId { get; set; } = senderId;
    
    public string SenderName { get; set; } = senderName;
    
    public string SenderAvatar { get; set; } = senderAvatar;

    public string Message { get; set; } = message;

    public int EntityId { get; set; } = entityId;
    
    public string EntityType { get; set; } = entityType;
    
    public DateTime CreatedAt { get; set; } = createdAt;
}