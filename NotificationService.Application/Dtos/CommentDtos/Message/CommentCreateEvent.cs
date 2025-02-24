namespace NotificationService.Application.Dtos.CommentDtos.Message;

public class CommentCreateEvent (int blogId)
{
    public int BlogId { get; set; } = blogId;
}