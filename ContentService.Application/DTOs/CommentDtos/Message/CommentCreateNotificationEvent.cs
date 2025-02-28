namespace ContentService.Application.DTOs.CommentDtos.Message;

public class CommentCreateNotificationEvent (int recipientId, int blogId, int commentId, string content, string cyclistName, string cyclistAvatar, DateTime createdAt)
{
    public int BlogId { get; set; } = blogId;
    
    public int CommentId { get; set; } = commentId;
    
    public string Content { get; set; } = content;
    
    public string CyclistName { get; set; } = cyclistName;
    
    public string CyclistAvatar { get; set; } = cyclistAvatar;
    
    public DateTime CreatedAt { get; set; } = createdAt;
}