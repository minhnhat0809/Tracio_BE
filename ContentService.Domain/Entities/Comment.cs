using ContentService.Domain.Enum;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ContentService.Domain.Entities;

public class Comment
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string CommentId { get; set; } = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("user_id")]
    public string UserId { get; set; } = null!;
    
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("entity_id")]
    public string EntityId { get; set; } = null!;
    
    [BsonElement("entity_type")]
    public EntityType EntityType { get; set; }

    [BsonElement("content")]
    public string Content { get; set; } = null!;
    
    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; }
    
    [BsonElement("is_edited")]
    public bool IsEdited { get; set; }
    
    [BsonElement("is_deleted")]
    public bool IsDeleted { get; set; }
    
    [BsonElement("likes_count")]
    public int LikesCount { get; set; }
}