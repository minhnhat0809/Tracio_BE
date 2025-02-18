namespace ContentService.Application.DTOs.CommentDtos.Message;

public class CommentCreatedEvent (int blogId)
{
    public int BlogId { get; set; } = blogId;
}