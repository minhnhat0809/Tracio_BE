using ContentService.Domain.Enum;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ContentService.Domain.Entities;

public class Reaction
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ReactionId { get; set; } = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string EntityId { get; set; } = null!;

    public EntityType EntityType { get; set; }
    
    public ReactionType ReactionType { get; set; }
    
    public DateTime CreatedAt { get; set; }
}