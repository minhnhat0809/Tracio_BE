using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NotificationService.Domain.Entities;

public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("notification_id")]
    public string NotificationId { get; set; } = null!;

    [BsonElement("recipient_id")]
    public int RecipientId { get; set; }
    
    [BsonElement("sender_id")]
    public int SenderId { get; set; }
    
    [BsonElement("sender_name")]
    public string SenderName { get; set; } = null!;

    [BsonElement("sender_avatar")]
    public string SenderAvatar { get; set; } = null!;

    [BsonElement("entity_id")]
    public int EntityId { get; set; }

    [BsonElement("entity_type")]
    [BsonRepresentation(BsonType.Int32)]
    public sbyte EntityType { get; set; } 

    [BsonElement("message")]
    public string Message { get; set; } = null!;

    [BsonElement("is_read")]
    public bool IsRead { get; set; }

    [BsonElement("is_deleted")]
    public bool IsDeleted { get; set; }

    [BsonElement("created_at")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
