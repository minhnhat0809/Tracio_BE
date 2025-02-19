namespace ContentService.Application.DTOs.CommentDtos.Message;

public class CommentCreateEvent (int blogId)
{
    public int BlogId { get; set; } = blogId;
}