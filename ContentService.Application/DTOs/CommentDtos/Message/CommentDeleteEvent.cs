namespace ContentService.Application.DTOs.CommentDtos.Message;

public class CommentDeleteEvent(int blogId)
{
    public int BlogId { get; set; } = blogId;
}