namespace ContentService.Domain.Events;

public class ReactionToReplyEvent(int replyId)
{
    public int ReplyId { get; set; } = replyId;
}