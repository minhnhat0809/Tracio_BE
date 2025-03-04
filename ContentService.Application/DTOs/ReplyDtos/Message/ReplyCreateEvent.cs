namespace ContentService.Application.DTOs.ReplyDtos.Message;

public class ReplyCreateEvent(int commentId)
{
    public int CommentId { get; set; } = commentId;
}