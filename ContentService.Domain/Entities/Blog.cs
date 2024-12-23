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
    [BsonElement("user_id")]
    public string UserId { get; set; } = null!;

    [BsonElement("tittle")]
    public string Tittle { get; set; } = null!;

    [BsonElement("content")]
    public string Content { get; set; } = null!;
    
    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; }
    
    [BsonElement("status")]
    public BlogStatus Status { get; set; }

    [BsonElement("likes_count")]
    public int LikesCount { get; set; } = 0;

    [BsonElement("comments_count")]
    public int CommentsCount { get; set; } = 0;
    
    [BsonElement("comments")]
    public List<Comment> Comments { get; set; } = null!;
}