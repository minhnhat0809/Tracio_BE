namespace ContentService.Application.DTOs.ReplyDtos.Message;

public class ReplyDeleteEvent(int commentId)
{
    public int CommentId { get; set; } = commentId;
}