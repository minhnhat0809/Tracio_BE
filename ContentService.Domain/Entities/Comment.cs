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
    public string UserId { get; set; } = null!;
    
    [BsonRepresentation(BsonType.ObjectId)]
    public string EntityId { get; set; } = null!;
    
    public EntityType EntityType { get; set; }

    public string Content { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public bool IsEdited { get; set; }
    
    public int LikesCount { get; set; }
}