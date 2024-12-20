using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ContentService.Domain.Entities;

public class Reply
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ReplyId { get; set; } = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string CommentId { get; set; } = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = null!;

    public string Content { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
    
    public int LikesCount { get; set; }
}