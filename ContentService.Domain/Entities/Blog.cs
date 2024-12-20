using ContentService.Domain.Enum;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ContentService.Domain.Entities;

public class Blog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string BlogId { get; set; } = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = null!;

    public string Tittle { get; set; } = null!;

    public string Content { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
    
    public BlogStatus Status { get; set; }

    public int LikesCount { get; set; } = 0;

    public int CommentsCount { get; set; } = 0;
}